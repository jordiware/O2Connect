namespace O2Connect.Api.Models.Options;

public class JwtOptions
{
    public required string Issuer { get; init; } = default!;
    public string ActiveKeyId { get; set; } = default!;
    public int AccessTokenLifetimeSeconds { get; init; } = 3600;
}
