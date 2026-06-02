using O2Connect.Api.Crypto;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using System.Collections.Immutable;
using System.Security.Claims;

namespace O2Connect.Api.Services;

public interface IAuthorizationService
{
    Task<AuthorizationResult> AuthorizeAsync(string sessionId,
                                             ClaimsPrincipal user,
                                             CancellationToken ct);
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
    private readonly IConsentService _consentService;
    private readonly ISecureTokenGenerator _secureTokenGenerator;

    public AuthorizationService(
        IClientRepository clientRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IAuthorizationSessionRepository authorizationSessionRepository,
        IConsentService consentService,
        ISecureTokenGenerator secureTokenGenerator)
    {
        _clientRepository = clientRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
        _consentService = consentService;
        _secureTokenGenerator = secureTokenGenerator;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(string sessionId,
                                                          ClaimsPrincipal user,
                                                          CancellationToken ct)
    {
        var session = await _authorizationSessionRepository.GetAsync(sessionId, ct);

        if (session == null)
            return Error("invalid_request", "Session already used or invalid", null);

        if (session.Stage != AuthorizationStage.ConsentRequired
            && session.Stage != AuthorizationStage.Ready)
            return Error("invalid_request", "Invalid session state", null);

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            return Error("invalid_request", "Session expired", null);

        var userId = user.FindFirst("sub")?.Value;

        if (session.UserId is null || session.UserId != userId)
            return Error("invalid_request", "Session does not belong to user", null);

        var consumedSession = await _authorizationSessionRepository.TryConsumeAsync(sessionId, ct);

        if (consumedSession == null)
            return Error("invalid_request", "Session already used or invalid", null);

        if (consumedSession.Stage == AuthorizationStage.Ready)
            return await IssueCodeAsync(consumedSession, ct);

        return await HandleAsync(session.Request, user, ct);
    }

    public async Task<AuthorizationResult> HandleAsync(AuthorizationRequest request,
                                                       ClaimsPrincipal user,
                                                       CancellationToken ct)
    {
        var sessionId = _secureTokenGenerator.GenerateSecureToken();

        var session = new AuthorizationSession
        {
            Id = sessionId,
            Request = request,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Stage = AuthorizationStage.Created
        };

        var validationError = ValidateRequest(request);

        if (validationError != null)
            return validationError;

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        if (client == null || !client.IsActive)
            return Error("invalid_client", "Client not found or inactive", request.State);

        var requestScopes = ValueSet.FromDataString(request.Scope, ' ').Values.ToHashSet();
        if (!requestScopes.IsSubsetOf(client.AllowedScopes))
            return Error("invalid_scope", "Scopes mismatch", request.State);

        if (!client.RedirectUris.Select(u => new Uri(u))
                                .Any(u => Uri.Compare(u,
                                                      new Uri(request.RedirectUri),
                                                      UriComponents.AbsoluteUri,
                                                      UriFormat.Unescaped,
                                                      StringComparison.Ordinal) == 0))
            return Error("invalid_request", "Client is not allowed to request these scopes", request.State);

        if (user?.Identity?.IsAuthenticated != true)
        {
            var loginSession = session with
            {
                Stage = AuthorizationStage.LoginRequired
            };
            await _authorizationSessionRepository.StoreAsync(loginSession, ct);

            var returnUrl = Uri.EscapeDataString($"{AuthorizeResumeUri}/{sessionId}");
            var loginRedirect = $"{LoginUri}?returnUrl={returnUrl}";

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

        session = session with
        {
            UserId = userId,
            RequestedScopes = requestScopes.ToImmutableHashSet()
        };

        var consent = await _consentService.EvaluateAsync(userId, client.ClientId, requestScopes, ct);

        if (consent.RequiresConsent)
        {
            var consentSession = session with
            {
                Stage = AuthorizationStage.ConsentRequired,
                MissingScopes = consent.MissingScopes.ToImmutableHashSet()
            };

            await _authorizationSessionRepository.StoreAsync(consentSession, ct);

            return new AuthorizationResult
            {
                Success = false,
                IsRedirect = true,
                RedirectUri = $"/consent?sessionId={sessionId}"
            };
        }

        session = session with { Stage = AuthorizationStage.Completed };
        await _authorizationSessionRepository.StoreAsync(session, ct);

        return await IssueCodeAsync(session, ct);
    }

    private async Task<AuthorizationResult> IssueCodeAsync(AuthorizationSession session, CancellationToken ct)
    {
        var code = _secureTokenGenerator.GenerateSecureToken();
        var request = session.Request;

        var authCode = new AuthorizationCode
        {
            Code = code,
            ClientId = request.ClientId,
            UserId = session.UserId!,
            RedirectUri = request.RedirectUri,
            Scopes = session.RequestedScopes.ToImmutableHashSet(),
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

    private AuthorizationResult Error(string code, string description, string? state, string? redirectUri = null)
    {
        return new AuthorizationResult
        {
            Success = false,
            Error = code,
            ErrorDescription = description,
            State = state,
            IsRedirect = !string.IsNullOrWhiteSpace(redirectUri),
            RedirectUri = redirectUri
        };
    }
}
