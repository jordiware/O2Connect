using O2Connect.Api.Crypto;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using System.Security.Claims;

namespace O2Connect.Api.Services;

public interface IAuthorizationService
{
    Task<AuthorizationResult> HandleAsync(AuthorizationRequest request,
                                          ClaimsPrincipal user,
                                          CancellationToken ct);
}

public class AuthorizationService : IAuthorizationService
{
    public const string LoginUri = "/account/login";
    public const string AuthorizeResumeUri = "/connect/authorize/resume";

    private readonly IClientRepository _clientRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IAuthorizationSessionRepository _authorizationSessionRepository;
    private readonly ISecureTokenGenerator _secureTokenGenerator;

    public AuthorizationService(
        IClientRepository clientRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IAuthorizationSessionRepository authorizationSessionRepository,
        ISecureTokenGenerator secureTokenGenerator)
    {
        _clientRepository = clientRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
        _secureTokenGenerator = secureTokenGenerator;
    }

    public async Task<AuthorizationResult> HandleAsync(AuthorizationRequest request,
                                                       ClaimsPrincipal user,
                                                       CancellationToken ct)
    {
        var validationError = ValidateRequest(request);
        if (validationError != null)
            return validationError;

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        if (client == null || !client.IsActive)
            return Error("invalid_client", "Client not found or inactive", request.State);

        var requestScopes = ValueSet.FromDataString(request.Scope, ' ');
        if (!requestScopes.IsSubsetOf(client.AllowedScopes))
            return Error("invalid_client", "Scopes mismatch", request.State);

        var requestUri = new Uri(request.RedirectUri);
        if (!client.RedirectUris.Any(u =>
        {
            var uri = new Uri(u);
            return uri.GetLeftPart(UriPartial.Path) == requestUri.GetLeftPart(UriPartial.Path);
        }))
            return Error("invalid_scope", "Client is not allowed to request these scopes", request.State);

        if (user?.Identity?.IsAuthenticated != true)
        {
            var sessionId = _secureTokenGenerator.GenerateSecureToken();

            var session = new AuthorizationSession
            {
                Id = sessionId,
                Request = request,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            await _authorizationSessionRepository.StoreAsync(session, ct);

            var loginRedirect = $"{LoginUri}?returnUrl={AuthorizeResumeUri}/{sessionId}";

            return new AuthorizationResult
            {
                Success = false,
                IsRedirect = true,
                RedirectUri = loginRedirect,
                Error = "login_required"
            };
        }

        var userId = user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Error("invalid_user", "User subject missing", request.State);

        var code = _secureTokenGenerator.GenerateSecureToken();

        var authCode = new AuthorizationCode
        {
            Code = code,
            ClientId = request.ClientId,
            UserId = userId,
            RedirectUri = request.RedirectUri,
            Scopes = requestScopes,
            CodeChallenge = request.CodeChallenge!,
            CodeChallengeMethod = request.CodeChallengeMethod,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        };

        await _authorizationCodeRepository.StoreAsync(authCode, ct);

        return new AuthorizationResult
        {
            Success = true,
            RedirectUri = request.RedirectUri,
            Code = code,
            State = request.State
        };
    }

    private AuthorizationResult? ValidateRequest(AuthorizationRequest request)
    {
        if (request.ResponseType != "code")
            return Error("unsupported_response_type", "Only 'code' is supported", request.State);

        if (string.IsNullOrWhiteSpace(request.ClientId))
            return Error("invalid_request", "client_id is required", request.State);

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            return Error("invalid_request", "redirect_uri is required", request.State);

        if (string.IsNullOrWhiteSpace(request.State))
            return Error("invalid_request", "state is required", request.State);

        if (string.IsNullOrWhiteSpace(request.Scope))
            return Error("invalid_scope", "scope must include 'openid'", request.State);

        var requestScopes = ValueSet.FromDataString(request.Scope, ' ');

        if (!requestScopes.Contains("openid"))
            return Error("invalid_scope", "scope must include 'openid'", request.State);

        if (string.IsNullOrWhiteSpace(request.CodeChallenge))
            return Error("invalid_request", "Code challenge missing", request.State);

        if (string.IsNullOrWhiteSpace(request.CodeChallengeMethod) || request.CodeChallengeMethod != "S256")
            return Error("invalid_request", "code_challenge_method is required", request.State);

        return null;
    }

    private AuthorizationResult Error(string code, string description, string? state)
    {
        return new AuthorizationResult
        {
            Success = false,
            Error = code,
            ErrorDescription = description,
            State = state
        };
    }
}
