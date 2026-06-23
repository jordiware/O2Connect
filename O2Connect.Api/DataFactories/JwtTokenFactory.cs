using O2Connect.Api.Config;
using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Dto.OidcOAuth.Connect;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace O2Connect.Api.DataFactories;

public interface ITokenFactory
{
    Task<TokenResponse> GenerateAsync(JwtTokenFactoryRequest request, CancellationToken ct);
}

public class JwtTokenFactory : ITokenFactory
{
    private readonly IJwtConfig _jwtConfig;
    private readonly ISigningKeyProvider _keyProvider;

    public JwtTokenFactory(
        IJwtConfig jwtConfig,
        ISigningKeyProvider keyProvider)
    {
        _jwtConfig = jwtConfig;
        _keyProvider = keyProvider;
    }

    public Task<TokenResponse> GenerateAsync(JwtTokenFactoryRequest request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        if (!_keyProvider.TryGetActiveKey(out var key))
            throw new InvalidOperationException("Active signing key required.");

        var claims = BuildClaims(request, now);

        var jwt = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: request.ClientId,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddSeconds(_jwtConfig.AccessTokenLifetimeSeconds).UtcDateTime,
            signingCredentials: key.Credentials);

        jwt.Header["kid"] = key.KeyId;

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return Task.FromResult(new TokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = _jwtConfig.AccessTokenLifetimeSeconds,
            Scope = string.Join(' ', request.Scopes.Order())
        });
    }

    private IEnumerable<Claim> BuildClaims(JwtTokenFactoryRequest request, DateTimeOffset now)
    {
        yield return new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
        yield return new Claim(JwtRegisteredClaimNames.Sub, request.Subject);
        yield return new Claim("client_id", request.ClientId);
        yield return new Claim("scope", string.Join(" ", request.Scopes));
        yield return new Claim(JwtRegisteredClaimNames.Iat,
                               now.ToUnixTimeSeconds().ToString(),
                               ClaimValueTypes.Integer64);

        if (request.AdditionalClaims != null)
        {
            foreach (var kv in request.AdditionalClaims)
            {
                yield return new Claim(kv.Key, Convert.ToString(kv.Value, CultureInfo.InvariantCulture)!);
            }
        }
    }
}
