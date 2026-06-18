using O2Connect.Dto.OidcOAuth.Connect;

namespace O2Connect.Api.Models.Mappers;

public static class AuthorizationRequestMapper
{
    public static AuthorizationRequestData ToData(this AuthorizationRequest request)
        => new()
        {
            ResponseType = request.ResponseType,
            ClientId = request.ClientId,
            RedirectUri = request.RedirectUri,
            Scope = request.Scope,
            State = request.State,
            Nonce = request.Nonce,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            ResponseMode = request.ResponseMode
        };

    public static AuthorizationRequestData ToData(this PushedAuthorizationRequest request)
        => new()
        {
            ResponseType = request.ResponseType,
            ClientId = request.ClientId,
            RedirectUri = request.RedirectUri,
            Scope = request.Scope,
            State = request.State,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,

            // Not present in PAR
            Nonce = null,
            ResponseMode = null
        };
}
