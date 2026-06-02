using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

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

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetConsent(string sessionId)
    {
        var session = await _consentService.GetSessionAsync(sessionId, HttpContext.RequestAborted);

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
            Scopes = session.MissingScopes
        };

        return Ok(response);
    }

    [HttpPost("{sessionId}")]
    public async Task<IActionResult> PostConsent(string sessionId, [FromBody] ConsentDecisionRequest request)
    {
        var session = await _consentService.GetSessionAsync(sessionId, HttpContext.RequestAborted);

        if (session == null 
            || session.Stage != AuthorizationStage.ConsentRequired 
            || session.ExpiresAt <= DateTimeOffset.UtcNow)
            return BadRequest("Consent session expired");

        if (session.MissingScopes == null || session.MissingScopes.Count == 0)
            return BadRequest("No consent required for this session");

        if (!request.Approved)
        {
            await _consentService.DeleteSessionAsync(sessionId, HttpContext.RequestAborted);

            return Ok(new RedirectResponse
            {
                Action = "deny",
                RedirectUrl = BuildErrorRedirect(session)
            });
        }

        if (request.ApprovedScopes == null || request.ApprovedScopes.Count == 0)
            return BadRequest("No scopes approved");

        if (!request.ApprovedScopes.All(session.MissingScopes.Contains))
            return BadRequest("Invalid scopes in approval");

        var scopesToPersist = request.ApprovedScopes;

        var sessionReady = await _consentService.TrySetReadySessionAsync(sessionId, HttpContext.RequestAborted);
        if (!sessionReady)
            return BadRequest("Session already used");

        await _consentService.SaveConsentAsync(session.UserId!,
                                               session.Request.ClientId,
                                               scopesToPersist,
                                               HttpContext.RequestAborted);

        return Ok(new RedirectResponse
        {
            Action = "resume",
            RedirectUrl = $"/connect/authorize/resume/{sessionId}"
        });
    }
}
