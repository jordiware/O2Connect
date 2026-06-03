namespace O2Connect.Api.Models.Store;

public sealed record User
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Username { get; init; } = default!;
    public string PasswordHash { get; init; } = default!;
    public string Role { get; init; } = default!;
    public ISet<string> Scopes { get; init; } = new HashSet<string>();
}
