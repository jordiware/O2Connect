using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace O2Connect.Api.DataValidators;

public interface IJwtValidator
{
    JwtValidationResult Validate(string token);
}

public class JwtValidator : IJwtValidator
{
    private static readonly HashSet<string> AllowedAlgs = new(StringComparer.Ordinal)
        { SecurityAlgorithms.RsaSha256 };

    private static readonly HashSet<string> ExcludedClaims = new(StringComparer.Ordinal)
        { "nbf", "exp", "iat", "iss", "aud", "sub", "client_id", "azp", "scope", "scp" };

    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly ILogger<JwtValidator> _logger;

    public JwtValidator(
        TokenValidationParameters tokenValidationParameters,
        ILogger<JwtValidator> logger)
    {
        _tokenValidationParameters = tokenValidationParameters;
        _logger = logger;
    }

    public JwtValidationResult Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return JwtValidationResult.Invalid;

        try
        {
            var principal = _tokenHandler.ValidateToken(token,
                                                        _tokenValidationParameters,
                                                        out var validatedToken);

            if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
                return JwtValidationResult.Invalid;

            if (validatedToken is not JwtSecurityToken jwt)
                return JwtValidationResult.Invalid;

            if (string.IsNullOrEmpty(jwt.Header.Alg))
                return JwtValidationResult.Invalid;

            if (!AllowedAlgs.Contains(jwt.Header.Alg))
                return JwtValidationResult.Invalid;

            if (jwt.Issuer != _tokenValidationParameters.ValidIssuer)
                return JwtValidationResult.Invalid;

            var clientId = principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value;

            if (string.IsNullOrWhiteSpace(clientId))
                return JwtValidationResult.Invalid;

            var subject = principal.FindFirst("sub")?.Value;
            var tokenType = principal.FindFirst("token_type")?.Value;

            var isClientCredentials = string.IsNullOrWhiteSpace(subject);

            if (string.IsNullOrWhiteSpace(tokenType))
            {
                if (isClientCredentials)
                    tokenType = "client_credentials";
                else
                    tokenType = "access_token";
            }

            if (isClientCredentials && !string.Equals(tokenType, "client_credentials", StringComparison.Ordinal))
                return JwtValidationResult.Invalid;

            if (!isClientCredentials && string.Equals(tokenType, "client_credentials", StringComparison.Ordinal))
                return JwtValidationResult.Invalid;

            var scopes = principal.FindAll(c => c.Type == "scope" || c.Type == "scp")
                                  .SelectMany(c => c.Value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries))
                                  .Distinct(StringComparer.Ordinal);

            if (!isClientCredentials && !scopes.Any())
                return JwtValidationResult.Invalid;

            var sessionId = principal.FindFirst("sid")?.Value;

            var nbf = principal.FindFirst("nbf")?.Value;
            if (!long.TryParse(nbf, out var parsedNbf))
                parsedNbf = new DateTimeOffset(validatedToken.ValidFrom).ToUnixTimeSeconds();

            var iat = principal.FindFirst("iat")?.Value;
            if (!long.TryParse(iat, out var parsedIat))
                parsedIat = parsedNbf;

            var exp = principal.FindFirst("exp")?.Value;
            if (!long.TryParse(exp, out var parsedExp))
                parsedExp = new DateTimeOffset(validatedToken.ValidTo).ToUnixTimeSeconds();

            var isValid = tokenType.Equals(tokenType, StringComparison.Ordinal);

            return new JwtValidationResult
            {
                IsValid = isValid,
                Subject = subject,
                ClientId = clientId,
                Scopes = scopes,
                SessionId = sessionId,
                TokenType = tokenType,
                NotBeforeUnix = parsedNbf,
                IatUnix = parsedIat,
                ExpUnix = parsedExp,
                Claims = principal.Claims.Where(c => !ExcludedClaims.Contains(c.Type))
                                         .GroupBy(c => c.Type)
                                         .ToDictionary(g => g.Key,
                                                       g => g.Select(x => x.Value).ToArray(),
                                                       StringComparer.Ordinal)
            };
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogDebug(ex, "JWT validation failed");
            return JwtValidationResult.Invalid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected JWT validation error");
            return JwtValidationResult.Invalid;
        }
    }
}
