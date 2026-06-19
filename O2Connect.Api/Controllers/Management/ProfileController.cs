using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Security;
using O2Connect.Api.Services.Management;
using O2Connect.Dto.Management.Profile;

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

    [HttpGet("consents")]
    [RequireScope(Scopes.Profile.Read)]
    public async Task<IActionResult> GetConsentedClients(CancellationToken ct)
    {
        var consents = await _profileService.GetConsentedClientsAsync(ct);

        return Ok(consents);
    }

    [HttpPatch("change_password")]
    [RequireScope(Scopes.Profile.Write)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request,
                                                    CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (string.IsNullOrWhiteSpace(request.OldPassword))
        {
            _logger.LogWarning("Old password is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "Old password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            _logger.LogWarning("New password is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "New password is required.");
        }

        await _profileService.UpdatePasswordAsync(request.OldPassword, request.NewPassword, ct);

        return NoContent();
    }

    [HttpPatch("revoke_consent")]
    [RequireScope(Scopes.Profile.Write)]
    public async Task<IActionResult> RevokeConsent([FromBody] RevokeConsentRequest request,
                                                    CancellationToken ct)
    {
        if (request is null || !ModelState.IsValid)
        {
            _logger.LogWarning("Missing or malformed request body.");
            throw ApiException.BadRequest("invalid_request_params", "Missing or malformed request body.");
        }

        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            _logger.LogWarning("Client ID is missing in the request.");
            throw ApiException.BadRequest("invalid_request_params", "Client ID is required.");
        }

        await _profileService.RevokeConsentedClientAsync(request.ClientId, ct);

        return NoContent();
    }
}
