using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.DataValidators;

public interface IPushedAuthorizationValidator
{
    void Validate(PushedAuthorizationRequest request, Client? client);
}

public sealed class PushedAuthorizationValidator : IPushedAuthorizationValidator
{
    public void Validate(PushedAuthorizationRequest request, Client? client)
    {
        ValidateClient(client);
        ValidateParAllowed(client!);
        ValidateResponseType(request, client!);
        ValidateRedirectUri(request, client!);
        ValidatePkce(request, client!);
        ValidateScope(request, client!);
    }

    private static void ValidateClient(Client? client)
    {
        if (client is null)
            throw OAuthException.FromInvalidClient();
    }

    private static void ValidateParAllowed(Client client)
    {
        if (!client.AllowPar)
            throw OAuthException.FromInvalidClient();
    }

    private static void ValidateResponseType(PushedAuthorizationRequest request, Client client)
    {
        if (!client.AllowedResponseTypes.Contains(request.ResponseType))
            throw OAuthException.FromUnsupportedResponseType();
    }

    private static void ValidateRedirectUri(PushedAuthorizationRequest request, Client client)
    {
        if (!client.RedirectUris.Contains(request.RedirectUri))
            throw OAuthException.FromInvalidRedirectUri();
    }

    private static void ValidatePkce(PushedAuthorizationRequest request, Client client)
    {
        if (client.RequiresPkce && string.IsNullOrWhiteSpace(request.CodeChallenge))
            throw OAuthException.FromInvalidRequest("code_challenge_required");

        if (!string.Equals(request.CodeChallengeMethod, "S256", StringComparison.Ordinal))
        {
            if (request.CodeChallengeMethod == "plain" && !client.AllowPlainPkce)
                throw OAuthException.FromInvalidRequest("plain_pkce_not_allowed");

            if (request.CodeChallengeMethod != "S256")
                throw OAuthException.FromInvalidRequest("unsupported_code_challenge_method");
        }

        // Optional but recommended: structural validation
        if (!string.IsNullOrWhiteSpace(request.CodeChallenge))
        {
            if (request.CodeChallenge.Length < 43 || request.CodeChallenge.Length > 128)
                throw OAuthException.FromInvalidRequest("invalid_code_challenge_length");
        }
    }

    private static void ValidateScope(PushedAuthorizationRequest request, Client client)
    {
        var scopes = request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var scope in scopes)
        {
            if (!client.AllowedScopes.Contains(scope))
                throw OAuthException.FromInvalidScope();
        }
    }
}
