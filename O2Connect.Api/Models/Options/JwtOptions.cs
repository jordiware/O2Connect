using O2Connect.Api.Helpers;

namespace O2Connect.Api.Models.Options;

public sealed record JwtOptions
{
    private string issuer = default!;
    public string ActiveKeyId { get; set; } = default!;
    public int AccessTokenLifetimeSeconds { get; init; } = 3600;

    public string Issuer
    {
        get => issuer;
        init => issuer = IssuerNormalizer.Normalize(value);
    }
}
