using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace O2Connect.Api.Controllers.OidcOAuth;

public abstract class OidcOAuthControllerBase : ControllerBase
{
    public IActionResult OkJsonResponse(object? response)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        return new JsonResult(response)
        {
            StatusCode = (int)HttpStatusCode.OK,
            ContentType = "application/json"
        };
    }
}
