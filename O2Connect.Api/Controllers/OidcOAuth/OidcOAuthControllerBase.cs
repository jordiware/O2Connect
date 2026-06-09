using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Controllers.OidcOAuth;

public abstract class OidcOAuthControllerBase : ControllerBase
{
    protected IActionResult OkJsonResponse(object? response)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        return new JsonResult(response)
        {
            StatusCode = StatusCodes.Status200OK,
            ContentType = "application/json"
        };
    }

    protected string BuildErrorRedirect(AuthorizationSession session)
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
