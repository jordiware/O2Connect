namespace O2Connect.Api.Exceptions;

public class ApiException : Exception
{
    public static ApiException BadRequest(string detail) =>
        new ApiException(StatusCodes.Status400BadRequest,
                         "Bad request",
                         detail,
                         "https://api.yourdomain.com/errors/bad-request");

    public static ApiException NotFound(string detail) =>
        new ApiException(StatusCodes.Status404NotFound,
                         "Resource not found",
                         detail,
                         "https://api.yourdomain.com/errors/not-found");

    public int StatusCode { get; }
    public string Title { get; }
    public string Type { get; }

    private ApiException(int statusCode, string title, string detail, string type) : base(detail)
    {
        StatusCode = statusCode;
        Title = title;
        Type = type;
    }
}
