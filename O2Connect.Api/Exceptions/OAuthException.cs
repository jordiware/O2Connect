namespace O2Connect.Api.Exceptions;

public class OAuthException : Exception
{
    public string Error { get; }
    public string? Description { get; }
    public string? Uri { get; }

    public OAuthException(string error, string? description = null, string? uri = null)
    {
        Error = error;
        Description = description;
        Uri = uri;
    }
}
