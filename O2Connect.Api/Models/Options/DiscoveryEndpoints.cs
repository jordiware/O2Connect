namespace O2Connect.Api.Models.Options;

public sealed record DiscoveryEndpoints
{
    public const string SectionName = "DiscoveryEndpoints";

    public string Documentation { get; init; } = default!;
    public string PrivacyPolicy { get; init; } = default!;
    public string TermsOfService { get; init; } = default!;
}