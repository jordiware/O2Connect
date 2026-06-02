using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using O2Connect.Api.Models.Store;
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

    public string BuildErrorRedirect(AuthorizationSession session)
    {
        return QueryHelpers.AddQueryString(
            session.Request.RedirectUri,
            new Dictionary<string, string?>
            {
                ["error"] = "access_denied",
                ["state"] = session.Request.State
            });
    }
}
