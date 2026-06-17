using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Security;
using O2Connect.Api.Services.OidcOAuth;
using O2Connect.Dto.Requests;
using System.Security.Claims;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect")]

public class ConnectDeviceController : ControllerBase
{
    private readonly IDeviceConnectService _deviceConnectService;

    public ConnectDeviceController(
        DeviceConnectService deviceAuthorizationService)
    {
        _deviceConnectService = deviceAuthorizationService;
    }

    [HttpPost("device_authorize")]
    [RequireClientToken]
    [RequireScope(Scopes.Clients.Read)]
    public async Task<IActionResult> DeviceAuthorize([FromForm] DeviceAuthorizationRequest request,
                                                     CancellationToken ct)
    {
        if (!Request.HasFormContentType)
            throw OAuthException.FromInvalidRequest();
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var response = await _deviceConnectService.CreateAsync(request.ClientId, request.Scope, ct);

        return Ok(response);
    }

    [HttpPost("device")]
    [RequireClientToken]
    public async Task<IActionResult> Device([FromQuery(Name = "user_code")] string userCode,
                                            [FromForm] DeviceDecisionRequest request,
                                            CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw OAuthException.FromInvalidRequest();

        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var userId = User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            throw OAuthException.FromInvalidGrant();

        await _deviceConnectService.ConsumeUserCodeAsync(userCode, request.Approved, userId, ct);

        return NoContent();
    }

    [HttpGet("device_status")]
    [RequireClientToken]
    [RequireScope(Scopes.Clients.Read)]
    public async Task<IActionResult> DeviceStatus([FromQuery(Name = "user_code")] string userCode,
                                                  CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw OAuthException.FromInvalidRequest();

        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var result = await _deviceConnectService.GetStatusAsync(userCode, ct);

        Response.Headers.CacheControl = "no-store, no-cache, max-age=0, must-revalidate";
        Response.Headers.Pragma = "no-cache";

        return Ok(result);
    }
}
