using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/token")]
public class TokenController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public TokenController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        if (!ModelState.IsValid)
            throw new OAuthException("invalid_request");

        var response = await _tokenService.HandleAsync(request, HttpContext.RequestAborted);

        return Ok(response);
    }
}
