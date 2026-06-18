using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Models;
using O2Connect.Api.Security;
using O2Connect.Api.Services.Management;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("management/me")]
public class ProfileController : ControllerBase
{
    private readonly IManagementProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IManagementProfileService profileService,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    [HttpGet]
    [RequireScope(Scopes.Profile.Read)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var me = await _profileService.GetMeAsync(ct);

        return Ok(me);
    }

    [HttpGet]
    [RequireScope(Scopes.Profile.Read)]
    public async Task<IActionResult> GetConsentedClients(CancellationToken ct)
    {
        var consents = await _profileService.GetConsentedClientsAsync(ct);

        return Ok(consents);
    }
}
