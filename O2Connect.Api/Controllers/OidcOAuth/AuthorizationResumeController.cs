using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/authorize/resume")]
public class AuthorizationResumeController : ControllerBase
{
    private readonly IAuthorizationSessionRepository _sessionRepository;
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationResumeController(
        IAuthorizationSessionRepository sessionRepository,
        IAuthorizationService authorizationService)
    {
        _sessionRepository = sessionRepository;
        _authorizationService = authorizationService;
    }

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> Resume(string sessionId, CancellationToken ct)
    {
        var session = await _sessionRepository.GetAsync(sessionId, ct);

        if (session == null || session.ExpiresAt < DateTimeOffset.UtcNow)
            return BadRequest("Session expired");

        var result = await _authorizationService.HandleAsync(session.Request, User, ct);

        await _sessionRepository.DeleteAsync(sessionId, ct);

        if (!result.Success)
        {
            return Redirect($"{session.Request.RedirectUri}?error={result.Error}&state={result.State}");
        }

        return Redirect($"{result.RedirectUri}?code={result.Code}&state={result.State}");
    }
}