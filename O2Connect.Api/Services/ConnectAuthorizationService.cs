using O2Connect.Api.Crypto;
using O2Connect.Api.DataFactories;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using System.Collections.Immutable;
using System.Security.Claims;

namespace O2Connect.Api.Services;

public interface IConnectAuthorizationService
{
    Task<AuthorizationResult> HandleAuthorizationAsync(AuthorizationRequestData request,
                                                       ClaimsPrincipal user,
                                                       CancellationToken ct,
                                                       AuthorizationSession? previousSession = null);
    Task<AuthorizationResult> HandleSessionAsync(string sessionId,
                                                 ClaimsPrincipal user,
                                                 CancellationToken ct);
}

public class ConnectAuthorizationService : IConnectAuthorizationService
{
    private readonly IClientRepository _clientRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IAuthorizationSessionRepository _authorizationSessionRepository;
    private readonly IConsentService _consentService;

    public ConnectAuthorizationService(
        IClientRepository clientRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IAuthorizationSessionRepository authorizationSessionRepository,
        IConsentService consentService)
    {
        _clientRepository = clientRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _authorizationSessionRepository = authorizationSessionRepository;
        _consentService = consentService;
    }

    public async Task<AuthorizationResult> HandleAuthorizationAsync(AuthorizationRequestData request,
                                                                    ClaimsPrincipal user,
                                                                    CancellationToken ct,
                                                                    AuthorizationSession? previousSession = null)
    {
        var responseMode = ExtractResponseMode(request);

        var requestScopes = previousSession?.RequestedScopes ?? ImmutableHashSet<string>.Empty;
        if (previousSession is null)
        {
            var validationError = ValidateRequest(request, out requestScopes);

            if (validationError != null)
                return validationError;

            if (requestScopes == null || requestScopes.Count == 0)
                return Error(responseMode, "invalid_scope", "Scopes are empty", request.State);
        }

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        if (client == null || !client.IsActive)
            return Error(responseMode, "invalid_client", "Client not found or inactive", request.State);

        if (!requestScopes.IsSubsetOf(client.AllowedScopes))
            return Error(responseMode, "invalid_scope", "Scopes mismatch", request.State);

        if (!client.RedirectUris.Select(u => new Uri(u))
                                .Any(u => Uri.Compare(u,
                                                      new Uri(request.RedirectUri),
                                                      UriComponents.AbsoluteUri,
                                                      UriFormat.Unescaped,
                                                      StringComparison.Ordinal) == 0))
            return Error(responseMode, "invalid_request", "Invalid redirect_uri", request.State);

        var session = previousSession;
        if (session is null)
        {
            session = new AuthorizationSession
            {
                SessionId = SecureCodeGenerator.GenerateBase64UrlToken(length: 32),
                Status = AuthorizationStatus.Initialized,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Request = request,
                ClientId = client.ClientId,
                ClientDisplayName = client.ClientName,
                RequestedScopes = requestScopes.ToHashSet()
            };
        }

        if (user?.Identity?.IsAuthenticated != true)
        {
            var loginSession = session with
            {
                SessionId = SecureCodeGenerator.GenerateBase64UrlToken(length: 32),
                Status = AuthorizationStatus.LoginRequired
            };

            await _authorizationSessionRepository.StoreAsync(loginSession, ct);

            var returnUrl = RedirectUrlFactory.AuthorizeResume(session: loginSession.SessionId);
            var loginRedirect = RedirectUrlFactory.LoginWithReturnUrl(returnUrl: returnUrl);

            return new AuthorizationResult
            {
                Success = false,
                IsRedirect = true,
                RedirectUri = loginRedirect,
                Error = "login_required",
                ResponseMode = responseMode
            };
        }

        var userId = user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Error(responseMode, "invalid_user", "User subject missing", request.State, request.RedirectUri);

        session = session with
        {
            UserId = userId,
        };

        var consent = await _consentService.EvaluateAsync(userId, client.ClientId, requestScopes, ct);

        if (consent.RequiresConsent)
        {
            var consentSession = session with
            {
                SessionId = SecureCodeGenerator.GenerateBase64UrlToken(length: 32),
                Status = AuthorizationStatus.ConsentRequired,
                MissingScopes = consent.MissingScopes?.ToImmutableHashSet()
            };

            await _authorizationSessionRepository.StoreAsync(consentSession, ct);

            return new AuthorizationResult
            {
                Success = false,
                IsRedirect = true,
                RedirectUri = RedirectUrlFactory.Consent(session: consentSession.SessionId),
                ResponseMode = responseMode
            };
        }

        return await IssueCodeAsync(session, ct);
    }

    public async Task<AuthorizationResult> HandleSessionAsync(string sessionId,
                                                              ClaimsPrincipal user,
                                                              CancellationToken ct)
    {
        var session = await _authorizationSessionRepository.TryConsumeAsync(sessionId, ct);

        if (session == null)
            return Error(AuthorizationResultResponseMode.Query, "invalid_request", "Session already used or invalid", null);

        var responseMode = ExtractResponseMode(session.Request);

        if (session.Status != AuthorizationStatus.Authenticated
            && session.Status != AuthorizationStatus.Consented)
            return Error(responseMode, "invalid_request", "Invalid session state", session.Request.State, session.Request.RedirectUri);

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            return Error(responseMode, "invalid_request", "Session expired", session.Request.State, session.Request.RedirectUri);

        var userId = user.FindFirst("sub")?.Value;

        if (session.UserId is null || session.UserId != userId)
            return Error(responseMode, "invalid_request", "Session does not belong to user", session.Request.State, session.Request.RedirectUri);

        if (session.Status == AuthorizationStatus.Consented)
            return await IssueCodeAsync(session, ct);

        return await HandleAuthorizationAsync(session.Request, user, ct, session);
    }

    private async Task<AuthorizationResult> IssueCodeAsync(AuthorizationSession session, CancellationToken ct)
    {
        var code = SecureCodeGenerator.GenerateBase64UrlToken(length: 32);

        var authCode = new AuthorizationCode
        {
            Code = code,
            ClientId = session.ClientId,
            UserId = session.UserId!,
            RedirectUri = session.Request.RedirectUri,
            Scopes = string.Join(' ', session.RequestedScopes ?? new HashSet<string>()).Trim(),
            CodeChallenge = session.Request.CodeChallenge!,
            CodeChallengeMethod = session.Request.CodeChallengeMethod!,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
        };

        await _authorizationCodeRepository.StoreAsync(authCode, ct);

        return new AuthorizationResult
        {
            Success = true,
            RedirectUri = session.Request.RedirectUri,
            Code = code,
            State = session.Request.State,
            ResponseMode = ExtractResponseMode(session.Request)
        };
    }

    private AuthorizationResult? ValidateRequest(AuthorizationRequestData request,
                                                 out IReadOnlySet<string>? requestScopes)
    {
        requestScopes = null;
        
        var responseMode = ExtractResponseMode(request);

        if (string.IsNullOrWhiteSpace(request.State))
            return Error(responseMode, "invalid_request", "state is required", null);

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            return Error(responseMode, "invalid_request", "redirect_uri is required", request.State);

        if (request.ResponseType != "code")
            return Error(responseMode, "unsupported_response_type", "Only 'code' is supported", request.State);

        if (string.IsNullOrWhiteSpace(request.ClientId))
            return Error(responseMode, "invalid_request", "client_id is required", request.State);

        if (string.IsNullOrWhiteSpace(request.Scope))
            return Error(responseMode, "invalid_scope", "scope must include 'openid'", request.State);

        requestScopes = ValueSet.FromDataString(request.Scope, ' ').Values;

        if (!requestScopes.Contains("openid"))
            return Error(responseMode, "invalid_scope", "scope must include 'openid'", request.State);

        if (string.IsNullOrWhiteSpace(request.CodeChallenge))
            return Error(responseMode, "invalid_request", "Code challenge missing", request.State);

        if (string.IsNullOrWhiteSpace(request.CodeChallengeMethod) || request.CodeChallengeMethod != "S256")
            return Error(responseMode, "invalid_request", "code_challenge_method is required", request.State);

        return null;
    }

    private AuthorizationResult Error(AuthorizationResultResponseMode responseMode,
                                      string code,
                                      string description,
                                      string? state,
                                      string? redirectUri = null)
    {
        return new AuthorizationResult
        {
            Success = false,
            Error = code,
            ErrorDescription = description,
            State = state,
            IsRedirect = !string.IsNullOrWhiteSpace(redirectUri),
            RedirectUri = redirectUri,
            ResponseMode = responseMode,
        };
    }

    private AuthorizationResultResponseMode ExtractResponseMode(AuthorizationRequestData request)
    {
        if (string.IsNullOrWhiteSpace(request.ResponseMode))
            return AuthorizationResultResponseMode.Query;

        return request.ResponseMode.ToLowerInvariant() switch
        {
            "fragment" => AuthorizationResultResponseMode.Fragment,
            "query" => AuthorizationResultResponseMode.Query,
            _ => AuthorizationResultResponseMode.Query
        };
    }
}
