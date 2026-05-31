using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public class AuthorizationCodeTokenRequestValidator : ITokenRequestValidator
{
    private static readonly Regex CodeVerifierRegex = new("^[A-Za-z0-9\\-._~]{43,128}$", RegexOptions.Compiled);

    private readonly IAuthorizationCodeRepository _authorizationCodeStore;

    public GrantType GrantType => GrantType.AuthorizationCode;

    public AuthorizationCodeTokenRequestValidator(IAuthorizationCodeRepository store)
    {
        _authorizationCodeStore = store;
    }

    public async Task<TokenRequestContext> ValidateAsync(TokenRequest request,
                                                         Client client,
                                                         ClientAuthenticationMethod method,
                                                         CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId)
            && method == ClientAuthenticationMethod.ClientSecretPost)
            throw OAuthException.FromInvalidRequest("Missing 'client_id'.");

        if (!string.IsNullOrWhiteSpace(request.ClientId) && request.ClientId != client.ClientId)
            throw OAuthException.FromInvalidGrant();

        if (string.IsNullOrWhiteSpace(request.Code))
            throw OAuthException.FromInvalidRequest("Missing 'code'.");

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            throw OAuthException.FromInvalidRequest("Missing 'redirect_uri'.");

        if (client.RequiresPkce && string.IsNullOrWhiteSpace(request.CodeVerifier))
            throw OAuthException.FromInvalidGrant();

        if (!string.IsNullOrWhiteSpace(request.CodeVerifier)
            && !CodeVerifierRegex.IsMatch(request.CodeVerifier))
            throw OAuthException.FromInvalidRequest("Invalid 'code_verifier'.");

        var code = await _authorizationCodeStore.GetAsync(request.Code, ct);
        if (code == null)
            throw OAuthException.FromInvalidGrant();

        if (code.ExpiresAt < DateTimeOffset.UtcNow)
            throw OAuthException.FromInvalidGrant();

        if (code.ClientId != client.ClientId)
            throw OAuthException.FromInvalidGrant();

        if (!string.Equals(code.RedirectUri, request.RedirectUri, StringComparison.Ordinal))
            throw OAuthException.FromInvalidGrant();

        if (!code.Scopes.IsSubsetOf(client.AllowedScopes))
            throw OAuthException.FromInvalidGrant();

        if (client.RequiresPkce && code.CodeChallenge.Length == 0)
            throw OAuthException.FromInvalidGrant();

        if (code.CodeChallenge.Length == 0)
        {
            if (string.IsNullOrEmpty(request.CodeVerifier))
                throw OAuthException.FromInvalidGrant();

            if (!PkceMethod.TryParse(code.CodeChallengeMethod, out var pkceMethod)
                || !PkceMethod.Supported.Contains(pkceMethod))
                throw OAuthException.FromInvalidGrant();

            var expected = TransformVerifier(request.CodeVerifier, pkceMethod);

            if (!CryptographicOperations.FixedTimeEquals(expected, code.CodeChallenge))
                throw OAuthException.FromInvalidGrant();
        }

        var context = new TokenRequestContext
        {
            AuthorizationCode = code,
            Client = client,
            ClientAuthenticationMethod = method,
            GrantType = GrantType,
            TokenRequest = request
        };

        if (!await _authorizationCodeStore.TryConsumeAsync(code.Code, ct))
            throw OAuthException.FromInvalidGrant();

        return context;
    }

    private static byte[] TransformVerifier(string codeVerifier, PkceMethod method)
    {
        var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);

        return method switch
        {
            var m when m == PkceMethod.Plain => codeVerifierBytes,
            var m when m == PkceMethod.S256 => Base64UrlEncode(SHA256.HashData(codeVerifierBytes)),
            _ => throw new InvalidOperationException("Unsupported PKCE method.")
        };
    }

    private static byte[] Base64UrlEncode(byte[] data)
    {
        var base64 = Convert.ToBase64String(data)
                            .Replace("+", "-")
                            .Replace("/", "_")
                            .TrimEnd('=');

        return Encoding.ASCII.GetBytes(base64);
    }
}
