using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostLogin([FromForm] LoginRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ReturnUrl) && !Url.IsLocalUrl(request.ReturnUrl))
            return BadRequest(new { message = "Invalid return URL" });

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return Unauthorized(new { message = "Invalid credentials" });

        var user = await _loginService.ValidateCredentialsAsync(request.Username.Trim(),
                                                                request.Password,
                                                                HttpContext.RequestAborted);

        if (user is null)
            return Unauthorized(new { message = "Invalid credentials" });

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
        };

        if (!string.IsNullOrWhiteSpace(user.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var now = DateTimeOffset.UtcNow;
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            AllowRefresh = false,
            IssuedUtc = now,
            ExpiresUtc = now.AddDays(30),
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                      principal,
                                      authProperties);

        if (!string.IsNullOrWhiteSpace(request.ReturnUrl))
            return LocalRedirect(request.ReturnUrl);

        return Ok(new { message = "Login successful" });
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
