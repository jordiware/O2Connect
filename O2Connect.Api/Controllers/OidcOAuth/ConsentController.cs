using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("consent")]
public class ConsentController : ControllerBase
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

        var response = new ConsentResponse
        {
            SessionId = sessionId,
            ClientId = session.Request.ClientId,
            ClientName = session.ClientDisplayName,
            UserDisplayName = session.UserDisplayName,
            Scopes = session.MissingScopes ?? session.RequestedScopes
        };

        return Ok(response);
    }

    [HttpPost("{sessionId}/{approved}")]
    public async Task<IActionResult> PostConsent(string sessionId, bool approved)
    {
        var session = await _consentService.GetSessionAsync(sessionId, HttpContext.RequestAborted);

        if (session == null || session.ExpiresAt <= DateTimeOffset.UtcNow)
            return BadRequest("Consent session expired");

        if (!approved)
        {
            return Redirect($"{session.Request.RedirectUri}?error=access_denied&state={session.Request.State}");
        }

        await _consentService.SaveConsentAsync(session.UserId!,
                                               session.Request.ClientId,
                                               session.RequestedScopes,
                                               HttpContext.RequestAborted);

        return Redirect($"/connect/authorize/resume/{sessionId}");
    }
}
