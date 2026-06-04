using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace O2Connect.Api.DataValidators;

public interface IJwtValidator
{
    JwtValidationResult? Validate(string token);
}

public class JwtValidator : IJwtValidator
{
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtValidator(
        TokenValidationParameters tokenValidationParameters)
    {
        _tokenValidationParameters = tokenValidationParameters;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public JwtValidationResult? Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var principal = _tokenHandler.ValidateToken(token,
                                                        _tokenValidationParameters,
                                                        out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt)
                return null;

            if (SecurityAlgorithms.RsaSha256 != jwt.Header.Alg)
                return null;

            var subject = principal.FindFirstValue("sub");
            var clientId = principal.FindFirstValue("client_id");
            var scope = principal.FindFirstValue("scope");
            var sessionId = principal.FindFirstValue("sid");

            if (subject is null || clientId is null)
                return null;

            return new JwtValidationResult
            {
                Subject = subject,
                ClientId = clientId,
                Scopes = scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [],
                SessionId = sessionId,
                ExpUnix = GetUnixClaim(principal, "exp"),
                IatUnix = GetUnixClaim(principal, "iat"),
                Claims = principal.Claims.ToDictionary(c => c.Type, c => (object)c.Value)
            };
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    private static long? GetUnixClaim(ClaimsPrincipal principal, string type)
    {
        var value = principal.FindFirstValue(type);
        return long.TryParse(value, out var result) ? result : null;
    }
}
