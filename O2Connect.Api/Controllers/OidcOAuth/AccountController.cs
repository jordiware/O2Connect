using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using System.Security.Claims;

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
    public async Task<IActionResult> PostLogin([FromForm] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Invalid credentials");

        if (!Url.IsLocalUrl(request.ReturnUrl))
            return BadRequest("Invalid return URL");

        var user = await _loginService.ValidateCredentialsAsync(request.Username, request.Password, HttpContext.RequestAborted);

        if (user is null)
            return BadRequest("Invalid credentials");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var identity = new ClaimsIdentity(claims, "cookie");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("cookie", principal);

        if (!string.IsNullOrWhiteSpace(request.ReturnUrl))
            return Redirect(request.ReturnUrl);

        return Ok();
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
