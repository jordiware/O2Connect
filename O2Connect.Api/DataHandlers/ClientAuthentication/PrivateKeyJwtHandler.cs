using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Options;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Cache;
using O2Connect.Dto.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace O2Connect.Api.DataHandlers.ClientAuthentication;

public class PrivateKeyJwtHandler : IClientAuthenticationHandler
{
    private readonly OAuthOptions _oauthOptions;
    private readonly IJwksProvider _jwksProvider;
    private readonly IReplayCache _replayCache;

    public ClientAuthenticationMethod Method => ClientAuthenticationMethod.PrivateKeyJwt;

    public PrivateKeyJwtHandler(
        IOptions<OAuthOptions> oauthOptions,
        IJwksProvider jwksProvider,
        IReplayCache replayCache)
    {
        _oauthOptions = oauthOptions.Value;
        _jwksProvider = jwksProvider;
        _replayCache = replayCache;
    }

    public bool CanHandle(HttpRequest request, TokenRequest tokenRequest)
    {
        return request.HasFormContentType &&
               request.Form["client_assertion_type"] == "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
    }

    public Task<string?> ExtractClientIdAsync(HttpRequest request, TokenRequest tokenRequest)
    {
        var assertion = tokenRequest.ClientAssertion;

        if (string.IsNullOrEmpty(assertion))
            return Task.FromResult<string?>(null);

        var jwtHandler = new JwtSecurityTokenHandler();

        if (!jwtHandler.CanReadToken(assertion))
            return Task.FromResult<string?>(null);

        try
        {
            var jwt = jwtHandler.ReadJwtToken(assertion);

            var sub = jwt.Subject;

            if (string.IsNullOrEmpty(jwt.Issuer) || jwt.Issuer != sub)
                return Task.FromResult<string?>(null);
            
            var clientId = jwt.Issuer;

            if (string.IsNullOrEmpty(clientId))
                return Task.FromResult<string?>(null);

            return Task.FromResult<string?>(clientId);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public async Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, Client client)
    {
        var assertion = tokenRequest.ClientAssertion;

        if (string.IsNullOrEmpty(assertion))
            return ClientAuthenticationResult.Fail();

        var principal = await ValidateJwt(assertion, client);

        if (principal == null)
            return ClientAuthenticationResult.Fail();

        var jti = principal.FindFirst("jti")?.Value;

        if (string.IsNullOrEmpty(jti))
            return ClientAuthenticationResult.Fail();

        var exp = principal.FindFirst("exp")?.Value;
        var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));

        if (!await _replayCache.TryAddAsync(jti, expiry))
            return ClientAuthenticationResult.Fail();

        return ClientAuthenticationResult.Success(client.ClientId);
    }

    private async Task<ClaimsPrincipal?> ValidateJwt(string jwt, Client client)
    {
        try
        {
            return await ValidateInternal(jwt, client, useFreshKeys: false);
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            _jwksProvider.Invalidate(client.JsonWebKeysUri!);

            return await ValidateInternal(jwt, client, useFreshKeys: true);
        }
    }

    private async Task<ClaimsPrincipal?> ValidateInternal(string jwt, Client client, bool useFreshKeys)
    {
        var handler = new JwtSecurityTokenHandler();

        var keys = await GetClientKeys(jwt, client, CancellationToken.None);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = client.ClientId,

            ValidateAudience = true,
            ValidAudience = _oauthOptions.TokenEndpoint,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,

            RequireSignedTokens = true,
            RequireExpirationTime = true,

            ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
        };

        return handler.ValidateToken(jwt, parameters, out _);
    }

    private async Task<IEnumerable<SecurityKey>> GetClientKeys(string jwt, Client client, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(client.JsonWebKeysUri))
            throw OAuthException.FromInvalidClient();

        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwt))
            throw OAuthException.FromInvalidClient();

        var token = handler.ReadJwtToken(jwt);

        var kid = token.Header.Kid;

        return await _jwksProvider.GetKeysAsync(client.JsonWebKeysUri, kid, ct);
    }
}
