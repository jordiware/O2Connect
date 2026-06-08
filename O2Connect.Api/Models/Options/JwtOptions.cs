using O2Connect.Api.Helpers;

namespace O2Connect.Api.Models.Options;

public sealed record JwtOptions
{
    public const string SectionName = "Jwt";

    private string issuer = default!;
    public required string ActiveKeyId { get; set; }
    public int AccessTokenLifetimeSeconds { get; init; } = 3600;

    public required string Issuer
    {
        get => issuer;
        init => issuer = IssuerNormalizer.Normalize(value);
    }
}
