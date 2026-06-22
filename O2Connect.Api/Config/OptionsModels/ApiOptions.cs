namespace O2Connect.Api.Config.OptionsModels;

public sealed record class ApiOptions
{
    public const string SectionName = "Api";

    public required string Domain { get; init; }
}
