using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Services;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/userinfo")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class UserInfoController : ControllerBase
{
    private readonly IUserInfoService _userInfoService;

    public UserInfoController(
        IUserInfoService userInfoService)
    {
        _userInfoService = userInfoService;
    }

    [HttpGet]
    public async Task<IActionResult> UserInfo()
    {
        var result = await _userInfoService.GetUserInfoAsync(User, HttpContext.RequestAborted);

        return Ok(result);
    }
}
