namespace O2Connect.Api.Models.Options;

public sealed record class ApiOptions
{
    public const string SectionName = "Api";

    public required string Domain { get; init; }
}
