using O2Connect.Dto.Responses;

namespace O2Connect.Api.Models;

public sealed record HandleResult<TResult>
{
    public HandleResultStatus Status { get; }
    public TResult? Result { get; }
    public OAuthErrorResponse? Error { get; }

    private HandleResult(HandleResultStatus status,
                         TResult? result = default,
                         OAuthErrorResponse? error = null)
    {
        Status = status;
        Result = result;
        Error = error;
    }

    public static HandleResult<TResult> Success(TResult result)
        => new(HandleResultStatus.Success, result: result);

    public static HandleResult<TResult> BadRequest(OAuthErrorResponse error)
        => new(HandleResultStatus.BadRequest, error: error);

    public static HandleResult<TResult> Unauthorized(OAuthErrorResponse error)
        => new(HandleResultStatus.Unauthorized, error: error);

    public static HandleResult<TResult> NotFound(OAuthErrorResponse error)
        => new(HandleResultStatus.NotFound, error: error);

    public static HandleResult<TResult> Forbidden(OAuthErrorResponse error)
        => new(HandleResultStatus.Forbidden, error: error);
}

public enum HandleResultStatus
{
    Success,
    BadRequest,
    Unauthorized,
    Forbidden,
    NotFound,
}
