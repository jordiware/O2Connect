namespace O2Connect.Api.Models.Options;

public class JwtOptions
{
    public required string Issuer { get; init; }
    public required string SigningKey { get; init; }
    public int AccessTokenLifetimeSeconds { get; init; } = 3600;
}
