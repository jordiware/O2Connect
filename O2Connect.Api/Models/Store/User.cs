namespace O2Connect.Api.Models.Store;

public sealed record User
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required string NormalizedUsername { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public required string Role { get; init; }
    public required IReadOnlySet<string> Scopes { get; init; }
    public required bool IsActive { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public string? DisplayName { get; init; }
    public string? PictureUri { get; init; }
}
