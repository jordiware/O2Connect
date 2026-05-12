namespace O2Connect.Dto.Responses;

public class OAuthErrorResponse
{
    public string Error { get; init; } = default!;
    public string? ErrorDescription { get; init; }
    public string? ErrorUri { get; init; }
}
