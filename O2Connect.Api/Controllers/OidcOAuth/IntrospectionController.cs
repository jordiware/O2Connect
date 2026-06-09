using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Services;
using O2Connect.Dto.Responses;
using System.Security.Claims;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/introspect")]
public class IntrospectionController : OidcOAuthControllerBase
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
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (string.IsNullOrWhiteSpace(token))
            return Ok(IntrospectionResponse.Inactive);

        var clientId = User.FindFirstValue("client_id");

        if (string.IsNullOrWhiteSpace(clientId))
            return Ok(IntrospectionResponse.Inactive);

        var result = await _tokenIntrospectionService.IntrospectAsync(token,
                                                                      clientId,
                                                                      HttpContext.RequestAborted);

        return Ok(result);
    }
}
