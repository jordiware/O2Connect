using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.RequestDtos;
using O2Connect.Api.Services;

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
            return BadRequest(ModelState);

        var response = _tokenService.HandleAsync(request);

        return Ok(response);
    }
}
