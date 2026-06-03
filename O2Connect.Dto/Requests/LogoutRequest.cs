namespace O2Connect.Dto.Requests;

public sealed record LogoutRequest
{
    public required string RefreshToken { get; init; }
}
