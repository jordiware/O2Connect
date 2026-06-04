using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services;
using System.Security.Claims;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/introspect")]
public class IntrospectionController : ControllerBase
{
    private readonly ITokenIntrospectionService _tokenIntrospectionService;

    public IntrospectionController(
        ITokenIntrospectionService tokenIntrospectionService)
    {
        _tokenIntrospectionService = tokenIntrospectionService;
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Client")]
    public async Task<IActionResult> Introspect([FromForm] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Ok(new { active = false });
        }

        var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _tokenIntrospectionService.IntrospectAsync(token,
                                                                      clientId!,
                                                                      HttpContext.RequestAborted);

        return Ok(result);
    }
}
