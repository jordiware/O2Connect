using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/userinfo")]
public class UserInfoController : ControllerBase
{
    [HttpGet]
    public IActionResult UserInfo()
    {
        return Ok("/connect/userinfo endpoint");
    }
}
