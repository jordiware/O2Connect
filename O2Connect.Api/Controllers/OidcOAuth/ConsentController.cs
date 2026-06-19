using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Services.OidcOAuth;
using O2Connect.Dto.OidcOAuth;
using O2Connect.Dto.OidcOAuth.Consent;

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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetConsent([FromQuery(Name = "session")] string sessionId,
                                                CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var response = _consentService.GetConsentResponse(sessionId, ct);

        return Ok(response);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostConsent([FromQuery(Name = "session")] string sessionId,
                                                 [FromBody] ConsentDecisionRequest request,
                                                 CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var result = default(RedirectResponse);

        if (await _consentService.IsParConsentSession(sessionId, ct))
            result = await _consentService.HandleParConsentAsync(sessionId, request, ct);
        else
            result = await _consentService.HandleConsentAsync(sessionId, request, ct);

        return Ok(result);
    }
}
