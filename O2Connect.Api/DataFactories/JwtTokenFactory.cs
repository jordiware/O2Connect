using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.Models.Options;
using O2Connect.Dto.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace O2Connect.Api.DataFactories;

public interface ITokenFactory
{
    Task<TokenResponse> GenerateAsync(JwtTokenFactoryRequest request, CancellationToken ct);
}

public class JwtTokenFactory : ITokenFactory
{
    private readonly JwtOptions _options;

    public JwtTokenFactory(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Task<TokenResponse> GenerateAsync(JwtTokenFactoryRequest request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var claims = BuildClaims(request);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: request.Client.ClientId,
            claims: claims,
            notBefore: now,
            expires: now.AddSeconds(_options.AccessTokenLifetimeSeconds),
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return Task.FromResult(new TokenResponse
        {
            AccessToken = token,
            ExpiresIn = _options.AccessTokenLifetimeSeconds,
            Scope = request.Scopes
        });

    }
    private static IEnumerable<Claim> BuildClaims(JwtTokenFactoryRequest request)
    {
        yield return new Claim(JwtRegisteredClaimNames.Sub, request.Subject);
        yield return new Claim("client_id", request.Client.ClientId);

        foreach (var scope in request.Scopes)
            yield return new Claim("scope", scope);

        if (request.AdditionalClaims != null)
        {
            foreach (var kv in request.AdditionalClaims)
            {
                yield return new Claim(kv.Key, kv.Value.ToString()!);
            }
        }
    }
}
