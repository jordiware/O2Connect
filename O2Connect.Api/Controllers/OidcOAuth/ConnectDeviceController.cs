using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect")]

public class ConnectDeviceController : ControllerBase
{
    private readonly IDeviceConnectService _deviceAuthorizationService;

    public ConnectDeviceController(
        DeviceConnectService deviceAuthorizationService)
    {
        _deviceAuthorizationService = deviceAuthorizationService;
    }

    [HttpPost("device_authorize")]
    public async Task<IActionResult> DeviceAuthorize([FromForm] DeviceAuthorizationRequest request,
                                                     CancellationToken ct)
    {
        if (!Request.HasFormContentType)
            throw OAuthException.FromInvalidRequest();
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var response = await _deviceAuthorizationService.CreateAsync(request.ClientId, request.Scope);

        return Ok(response);
    }

    [HttpPost("device")]
    public async Task<IActionResult> Device(CancellationToken ct)
    {
        return NoContent();
    }

    [HttpPost("device_status")]
    public async Task<IActionResult> DeviceStatus(CancellationToken ct)
    {
        return NoContent();
    }
}
