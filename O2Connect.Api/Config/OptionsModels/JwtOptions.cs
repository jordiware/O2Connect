namespace O2Connect.Api.Config.OptionsModels;

public sealed record JwtOptions
{
    public const string SectionName = "Jwt";
 
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int AccessTokenLifetimeSeconds { get; init; }
    public int RefreshTokenLifetimeDays { get; init; }
    public required SigningOptions Signing { get; init; }
}

public sealed record SigningOptions
{
    public required string KeyId { get; init; }
    public required string PrivateKeyPath { get; init; }
    public required string PublicKeyPath { get; init; }
}
