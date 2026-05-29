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
    private const string JwtBearerAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

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

    public bool CanAuthenticate(HttpRequest request, TokenRequest tokenRequest)
    {
        return request.HasFormContentType
               && !string.IsNullOrWhiteSpace(tokenRequest.ClientAssertion)
               && !string.IsNullOrWhiteSpace(tokenRequest.ClientAssertionType)
               && tokenRequest.ClientAssertionType == JwtBearerAssertionType;
    }

    public async Task<string> ExtractClientIdAsync(HttpRequest request, TokenRequest tokenRequest, CancellationToken ct)
    {
        var assertion = tokenRequest.ClientAssertion;

        if (string.IsNullOrEmpty(assertion))
            throw OAuthException.FromInvalidRequest();

        var jwtHandler = new JwtSecurityTokenHandler();

        if (!jwtHandler.CanReadToken(assertion))
            throw OAuthException.FromInvalidRequest();

        try
        {
            var jwt = jwtHandler.ReadJwtToken(assertion);

            var sub = jwt.Subject;

            if (string.IsNullOrEmpty(jwt.Issuer) || jwt.Issuer != sub)
                throw OAuthException.FromInvalidRequest();

            var clientId = jwt.Issuer;

            if (string.IsNullOrEmpty(clientId))
                throw OAuthException.FromInvalidClient();

            return clientId;
        }
        catch
        {
            throw OAuthException.FromInvalidRequest();
        }
    }

    public async Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, Client client, CancellationToken ct)
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
        var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp!));

        if (!await _replayCache.TryAddAsync(jti, expiry))
            return ClientAuthenticationResult.Fail();

        return ClientAuthenticationResult.Ok(client, Method);
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

    private string? GetKid(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwt))
            return null;

        var token = handler.ReadJwtToken(jwt);

        return token.Header.Kid;
    }
}
