using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Config;
using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Cache;
using O2Connect.Dto.OidcOAuth.Connect;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace O2Connect.Api.DataHandlers.ClientAuthentication;

public class PrivateKeyJwtHandler : IClientAuthenticationHandler
{
    private const string JwtBearerAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

    private readonly IOidcConfig _oidcConfig;
    private readonly IJwksProvider _jwksProvider;
    private readonly IReplayCache _replayCache;

    public ClientAuthenticationMethod Method => ClientAuthenticationMethod.PrivateKeyJwt;

    public PrivateKeyJwtHandler(
        IOidcConfig oidcConfig,
        IJwksProvider jwksProvider,
        IReplayCache replayCache)
    {
        _oidcConfig = oidcConfig;
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

    public void ValidateSingleCredentialsSource(HttpRequest request, TokenRequest tokenRequest)
    {
        if (!string.IsNullOrWhiteSpace(tokenRequest.ClientId)
            || !string.IsNullOrWhiteSpace(tokenRequest.ClientSecret)
            || request.Headers.Authorization.Count != 0)
            throw OAuthException.FromInvalidRequest("Multiple Authorization sources");
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

            if (!string.IsNullOrEmpty(jwt.Subject) && jwt.Subject != jwt.Issuer)
                throw OAuthException.FromInvalidRequest();

            var clientId = jwt.Issuer;

            if (string.IsNullOrEmpty(clientId))
                throw OAuthException.FromInvalidRequest();

            return clientId;
        }
        catch (ArgumentException) { throw OAuthException.FromInvalidRequest(); }
        catch (SecurityTokenException) { throw OAuthException.FromInvalidRequest(); }
    }

    public async Task<ClientAuthenticationResult> AuthenticateAsync(HttpRequest request, TokenRequest tokenRequest, Client client, CancellationToken ct)
    {
        var assertion = tokenRequest.ClientAssertion;

        if (string.IsNullOrEmpty(assertion))
            return ClientAuthenticationResult.Fail();

        if (string.IsNullOrEmpty(client.JsonWebKeysUri))
            return ClientAuthenticationResult.Fail();

        var parsed = ParseAssertion(assertion, client);

        await ValidateJwt(parsed, client, ct);

        if (string.IsNullOrEmpty(parsed.Jti))
            return ClientAuthenticationResult.Fail();

        if (!parsed.Exp.HasValue)
            return ClientAuthenticationResult.Fail();

        var now = DateTimeOffset.UtcNow;
        var expiry = DateTimeOffset.FromUnixTimeSeconds(parsed.Exp.Value);

        if (expiry < now)
            return ClientAuthenticationResult.Fail();

        var maxLifetime = TimeSpan.FromMinutes(5);

        if (expiry > now.Add(maxLifetime))
            return ClientAuthenticationResult.Fail();

        if (!parsed.Token.Payload.TryGetValue("iat", out var iatObj)
            || !long.TryParse(iatObj?.ToString(), out var iat))
            return ClientAuthenticationResult.Fail();

        var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat);

        if (issuedAt > now.AddMinutes(1))
            return ClientAuthenticationResult.Fail();

        if (issuedAt < now.Subtract(maxLifetime))
            return ClientAuthenticationResult.Fail();

        if (!await _replayCache.TryAddAsync(parsed.Jti, expiry))
            return ClientAuthenticationResult.Fail();

        return ClientAuthenticationResult.Ok(client, Method);
    }

    private ParsedClientAssertion ParseAssertion(string assertion, Client client)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(assertion))
            throw OAuthException.FromInvalidRequest();

        var jwt = handler.ReadJwtToken(assertion);

        if (!string.IsNullOrEmpty(jwt.Issuer) && jwt.Issuer != client.Id)
            throw OAuthException.FromInvalidRequest();

        if (!string.IsNullOrEmpty(jwt.Subject) && jwt.Subject != jwt.Issuer)
            throw OAuthException.FromInvalidRequest();

        var clientId = jwt.Issuer;

        if (string.IsNullOrEmpty(clientId))
            throw OAuthException.FromInvalidRequest();

        long? exp = jwt.Payload.TryGetValue("exp", out var value)
                    && long.TryParse(value?.ToString(), out var parsed)
                        ? parsed
                        : null;

        return new ParsedClientAssertion(clientId, jwt.Id, exp, jwt);
    }

    private async Task<ClaimsPrincipal> ValidateJwt(ParsedClientAssertion parsedClientAssertion, Client client, CancellationToken ct)
    {
        try
        {
            return await ValidateInternal(parsedClientAssertion, client, ct);
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            _jwksProvider.Invalidate(client.JsonWebKeysUri!);


            try
            {
                return await ValidateInternal(parsedClientAssertion, client, ct);
            }
            catch
            {
                throw OAuthException.FromInvalidClient();
            }
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _jwksProvider.Invalidate(client.JsonWebKeysUri!);

            try
            {
                return await ValidateInternal(parsedClientAssertion, client, ct);
            }
            catch
            {
                throw OAuthException.FromInvalidClient();
            }
        }
    }

    private async Task<ClaimsPrincipal> ValidateInternal(ParsedClientAssertion parsedClientAssertion, Client client, CancellationToken ct)
    {
        var jwt = parsedClientAssertion.Token;

        if (jwt.Header.Alg != SecurityAlgorithms.RsaSha256)
            throw OAuthException.FromInvalidClient();

        var keys = await GetClientKeys(parsedClientAssertion, client, ct);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = client.Id,

            ValidateAudience = true,
            ValidAudience = _oidcConfig.TokenEndpoint,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys,

            RequireSignedTokens = true,
            RequireExpirationTime = true,

            ValidAlgorithms = [SecurityAlgorithms.RsaSha256]
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(parsedClientAssertion.Token.RawData, parameters, out _);
    }

    private async Task<IEnumerable<SecurityKey>> GetClientKeys(ParsedClientAssertion parsedClientAssertion, Client client, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(client.JsonWebKeysUri))
            throw OAuthException.FromInvalidClient();

        var kid = parsedClientAssertion.Token.Header.Kid;

        return await _jwksProvider.GetKeysAsync(client.JsonWebKeysUri, kid, SecurityAlgorithms.RsaSha256, ct);
    }

    private sealed record ParsedClientAssertion(string ClientId, string? Jti, long? Exp, JwtSecurityToken Token);
}
