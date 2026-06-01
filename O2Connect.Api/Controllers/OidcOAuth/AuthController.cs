using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("account")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login()
    {
        return Ok("/auth/login endpoint");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok("/auth/logout endpoint");
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok("/auth/me endpoint");
    }
}
