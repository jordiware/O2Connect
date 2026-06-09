using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/revocation")]
public class RevocationController : ControllerBase
{
    private readonly IRevocationService _revocationService;

    public RevocationController(IRevocationService revocationService)
    {
        _revocationService = revocationService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Revoke([FromForm] RevocationRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var clientId = ExtractClientId(HttpContext);

        await _revocationService.HandleAsync(request, clientId, ct);

        return Ok();
    }

    private static string? ExtractClientId(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated != true)
            return null;

        var clientId = user.FindFirst("client_id")?.Value;

        if (!string.IsNullOrWhiteSpace(clientId))
            return clientId;

        var azp = user.FindFirst("azp")?.Value;

        if (!string.IsNullOrWhiteSpace(azp))
            return azp;

        return null;
    }
}
