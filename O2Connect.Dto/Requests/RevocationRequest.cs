namespace O2Connect.Dto.Requests;

public sealed record RevocationRequest
{
    public string Token { get; init; } = default!;
    public string? TokenTypeHint { get; init; }
}
