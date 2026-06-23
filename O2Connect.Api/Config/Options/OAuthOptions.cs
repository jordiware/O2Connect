namespace O2Connect.Api.Config.Options;

public sealed record OAuthOptions
{
    public const string SectionName = "OAuth";

    public bool RequirePkce { get; init; }
    public bool AllowPlainPkce { get; init; }
    public bool AllowRefreshTokenReuse { get; init; }
    public bool RotateRefreshTokens { get; init; }
    public bool ReuseDetectionEnabled { get; init; }
}
