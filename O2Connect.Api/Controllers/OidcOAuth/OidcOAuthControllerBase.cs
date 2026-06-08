using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using System.Net;

namespace O2Connect.Api.Controllers.OidcOAuth;

public abstract class OidcOAuthControllerBase : ControllerBase
{
    protected IActionResult OkJsonResponse(object? response)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        return new JsonResult(response)
        {
            StatusCode = (int)HttpStatusCode.OK,
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

    protected IActionResult ProcessHandleResult<TResult>(HandleResult<TResult> handleResult,
                                                         Func<TResult, IActionResult> OnSuccess)
    {
        if (handleResult == null || OnSuccess == null)
            return StatusCode(500);

        return handleResult.Status switch
        {
            HandleResultStatus.Success => OnSuccess(handleResult.Result!),
            HandleResultStatus.BadRequest => BadRequest(handleResult.Error),
            HandleResultStatus.Unauthorized => Unauthorized(handleResult.Error),
            HandleResultStatus.NotFound => NotFound(handleResult.Error),
            HandleResultStatus.Forbidden => Forbid(),
            _ => StatusCode(500)
        };
    }
}
