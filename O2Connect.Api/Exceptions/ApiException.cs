namespace O2Connect.Api.Exceptions;

public class ApiException : Exception
{
    public static ApiException BadRequest(string errorCode, string detail) =>
        new ApiException(StatusCodes.Status400BadRequest,
                         errorCode,
                         "Bad request",
                         "/errors/bad-request",
                         detail);

    public static ApiException NotFound(string errorCode, string detail) =>
        new ApiException(StatusCodes.Status404NotFound,
                         errorCode,
                         "Resource not found",
                         "/errors/not-found",
                         detail);

    public static ApiException Conflict(string errorCode, string detail) =>
        new ApiException(StatusCodes.Status409Conflict,
                         errorCode,
                         "Bad request",
                         "/errors/conflict",
                         detail);

    public int StatusCode { get; }
    public string ErrorCode { get; }
    public string Title { get; }
    public string Type { get; }

    private ApiException(int statusCode, string errorCode, string title, string type, string detail) 
        : base(detail)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Title = title;
        Type = type;
    }
}
