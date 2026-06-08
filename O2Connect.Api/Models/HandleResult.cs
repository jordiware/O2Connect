namespace O2Connect.Api.Models;

public sealed record HandleResult<TResult> where TResult : struct
{
    public HandleResultStatus Status { get; }
    public TResult? Result { get; }
    public string? ErrorMessage { get; }

    private HandleResult(HandleResultStatus status,
                         TResult? result = default,
                         string? errorMessage = null)
    {
        Status = status;
        Result = result;
        ErrorMessage = errorMessage;
    }

    public static HandleResult<TResult> Success(TResult result)
        => new(HandleResultStatus.Success, result: result);

    public static HandleResult<TResult> BadRequest(string errorMessage)
        => new(HandleResultStatus.BadRequest, errorMessage: errorMessage);

    public static HandleResult<TResult> Unauthorized(string errorMessage)
        => new(HandleResultStatus.Unauthorized, errorMessage: errorMessage);

    public static HandleResult<TResult> Forbidden()
        => new(HandleResultStatus.Forbidden);

    public static HandleResult<TResult> NotFound(string errorMessage)
        => new(HandleResultStatus.NotFound, errorMessage: errorMessage);
}

public enum HandleResultStatus
{
    Success,
    BadRequest,
    Unauthorized,
    Forbidden,
    NotFound,
}
