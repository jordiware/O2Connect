using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly ILoginService _loginService;

    public AccountController(
        ILoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpGet("login")]
    public IActionResult GetLogin([FromQuery] string? returnUrl)
    {
        return Ok("/auth/login endpoint");
    }

    [HttpPost("login")]
    public IActionResult PostLogin([FromForm] LoginRequest request)
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
