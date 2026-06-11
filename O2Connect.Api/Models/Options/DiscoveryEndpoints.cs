namespace O2Connect.Api.Models.Options;

public sealed record DiscoveryEndpoints
{
    public const string SectionName = "DiscoveryEndpoints";

    public required string Documentation { get; init; }
    public required string PrivacyPolicy { get; init; }
    public required string TermsOfService { get; init; }
}