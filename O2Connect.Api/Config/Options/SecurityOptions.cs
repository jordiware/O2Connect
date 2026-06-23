namespace O2Connect.Api.Config.Options;

public sealed record SecurityOptions
{
    public const string SectionName = "Security";

    public required PasswordOptions Password { get; init; }
    public required LockoutOptions Lockout { get; init; }
}

public sealed record PasswordOptions
{
    public int MinLength { get; init; }
    public bool RequireUppercase { get; init; }
    public bool RequireLowercase { get; init; }
    public bool RequireDigit { get; init; }
    public bool RequireNonAlphanumeric { get; init; }
}

public sealed record LockoutOptions
{
    public int MaxFailedAttempts { get; init; }
    public int DurationMinutes { get; init; }
}
