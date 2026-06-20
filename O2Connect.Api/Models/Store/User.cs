namespace O2Connect.Api.Models.Store;

public sealed record User
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public required string NormalizedUsername { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public required string Role { get; init; }
    public required string[] Scopes { get; init; }
    public required EntityStatus Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
    public DateTimeOffset? RevokedAt { get; init; }

    public string? DisplayName { get; init; }
    public string? ImageUrl { get; init; }

    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<UserConsent> Consents { get; set; } = new List<UserConsent>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AuthorizationCode> AuthorizationCodes { get; set; } = new List<AuthorizationCode>();
    public ICollection<DeviceAuthorization> DeviceAuthorizations { get; set; } = new List<DeviceAuthorization>();
}
