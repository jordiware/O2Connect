using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.DataFactories;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Collections.Immutable;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("consent")]
public class ConsentController : OidcOAuthControllerBase
{
    private readonly IConsentService _consentService;

    public ConsentController(
        IConsentService consentService)
    {
        _consentService = consentService;
    }

    [HttpGet]
    [Authorize(Policy = "RequireProfileScope")]
    public async Task<IActionResult> GetConsent([FromQuery(Name = "session")] string? sessionId,
                                                CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(sessionId))
            throw OAuthException.FromInvalidRequest();

        var session = await _consentService.GetSessionAsync(sessionId, ct);

        if (session == null || session.ExpiresAt <= DateTimeOffset.UtcNow)
            return BadRequest("Consent session expired");

        if (session.MissingScopes == null || session.MissingScopes.Count == 0)
            return BadRequest("No consent required for this session");

        var response = new ConsentResponse
        {
            SessionId = sessionId,
            ClientId = session.Request.ClientId,
            ClientName = session.ClientDisplayName,
            UserDisplayName = session.UserDisplayName,
            Scope = string.Join(' ', session.MissingScopes.Order())
        };

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = "RequireProfileScope")]
    public async Task<IActionResult> PostConsent([FromQuery(Name = "session")] string? sessionId,
                                                 [FromBody] ConsentDecisionRequest request,
                                                 CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(sessionId))
            throw OAuthException.FromInvalidRequest();

        var session = await _consentService.GetSessionAsync(sessionId, ct);

        if (session == null 
            || session.Stage != AuthorizationStage.ConsentRequired 
            || session.ExpiresAt <= DateTimeOffset.UtcNow)
            return BadRequest("Consent session expired");

        if (session.MissingScopes == null || session.MissingScopes.Count == 0)
            return BadRequest("No consent required for this session");

        if (!request.Approved)
        {
            await _consentService.DeleteSessionAsync(sessionId, ct);

            return Ok(new RedirectResponse
            {
                Action = "deny",
                RedirectUrl = BuildErrorRedirect(session)
            });
        }

        if (string.IsNullOrWhiteSpace(request.ApprovedScopes) 
            || request.ApprovedScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length == 0)
            return BadRequest("No scopes approved");

        if (!request.ApprovedScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                   .All(session.MissingScopes.Contains))
            return BadRequest("Invalid scopes in approval");

        var sessionReady = await _consentService.SetConsentGrantedSessionAsync(sessionId, ct);
        
        if (!sessionReady)
            return BadRequest("Session already used");

        var scopesToPersist = request.ApprovedScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToImmutableHashSet();

        await _consentService.SaveConsentAsync(session.UserId!,
                                               session.Request.ClientId,
                                               scopesToPersist,
                                               ct);

        return Ok(new RedirectResponse
        {
            Action = "resume",
            RedirectUrl = RedirectUrlFactory.AuthorizeResume(session: sessionId)
        });
    }

    [HttpPost]
    public async Task<IActionResult> PostParConsent([FromQuery(Name = "session")] string sessionId,
                                                    [FromBody] ConsentDecisionRequest request,
                                                    CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var result = await _consentService.HandleParConsentAsync(sessionId,
                                                                 request,
                                                                 ct);

        return Ok(result);
    }
}
