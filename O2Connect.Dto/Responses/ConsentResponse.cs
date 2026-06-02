namespace O2Connect.Dto.Responses;

public sealed record ConsentResponse
{
    public string SessionId { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public string? ClientName { get; init; }
    public IEnumerable<string> Scopes { get; init; } = [];
    public string? UserDisplayName { get; init; }
}
