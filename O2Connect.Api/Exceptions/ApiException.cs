namespace O2Connect.Api.Exceptions;

public class ApiException : Exception
{
    public static ApiException BadRequest(string errorCode, string detail) =>
        new ApiException(StatusCodes.Status400BadRequest,
                         errorCode,
                         "Bad request",
                         "https://api.yourdomain.com/errors/bad-request",
                         detail);

    public static ApiException NotFound(string errorCode, string detail) =>
        new ApiException(StatusCodes.Status404NotFound,
                         errorCode,
                         "Resource not found",
                         "https://api.yourdomain.com/errors/not-found",
                         detail);

    public int StatusCode { get; }
    public string Title { get; }
    public string Type { get; }
    public string ErrorCode { get; }

    private ApiException(int statusCode, string errorCode, string title, string type, string detail) 
        : base(detail)
    {
        StatusCode = statusCode;
        Title = title;
        Type = type;
        ErrorCode = errorCode;
    }
}
