namespace O2Connect.Api.Config.Options;

public sealed record DatabaseOptions
{
    public const string SectionName = "Database";

    public required string Provider { get; init; }
    public required string ConnectionString { get; init; }
}
