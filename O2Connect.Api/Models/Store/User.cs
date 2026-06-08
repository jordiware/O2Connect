namespace O2Connect.Api.Models.Store;

public sealed record User
{
    public required string Id { get; init; } = Guid.NewGuid().ToString();
    public required string Username { get; init; }
    public required string PasswordHash { get; init; }
    public required IReadOnlySet<string> Roles { get; init; }
    public required IReadOnlySet<string> Scopes { get; init; }
}
