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
        { "nbf", "exp", "iat", "iss", "aud" };

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

            if (validatedToken is not JwtSecurityToken jwt)
                return JwtValidationResult.Invalid;

            if (!AllowedAlgs.Contains(jwt.Header.Alg))
                return JwtValidationResult.Invalid;

            var clientId = principal.FindFirstValue("client_id") ?? principal.FindFirstValue("azp");

            if (clientId is null)
                return JwtValidationResult.Invalid;

            var subject = principal.FindFirstValue("sub");
            var scope = principal.FindFirstValue("scope") ?? principal.FindFirstValue("scp");
            var sessionId = principal.FindFirstValue("sid");
            var tokenType = principal.FindFirstValue("token_type");

            if (string.IsNullOrWhiteSpace(tokenType))
            {
                if (subject != null)
                    tokenType = "access_token";
                else 
                    tokenType = "client_credentials";

            }

            var isClientCredentials = subject is null;

            if (isClientCredentials && tokenType != "client_credentials")
                return JwtValidationResult.Invalid;

            if (!isClientCredentials && tokenType == "client_credentials")
                return JwtValidationResult.Invalid;

            return new JwtValidationResult
            {
                IsValid = true,
                Subject = subject,
                ClientId = clientId,
                Scopes = scope?.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries) ?? [],
                SessionId = sessionId,
                TokenType = tokenType,
                ExpUnix = jwt.Payload.Expiration,
                IatUnix = jwt.Payload.IssuedAt == DateTime.MinValue
                          ? null 
                          : new DateTimeOffset(jwt.Payload.IssuedAt).ToUnixTimeSeconds(),
                NotBeforeUnix = jwt.Payload.NotBefore,
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
