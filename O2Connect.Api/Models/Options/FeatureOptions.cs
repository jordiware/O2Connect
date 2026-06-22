namespace O2Connect.Api.Models.Options;

public sealed record FeatureOptions
{
    public const string SectionName = "Features";

    public bool EnableDynamicClientRegistration { get; init; }
    public bool EnableDeviceFlow { get; init; }
    public bool EnableRevocationEndpoint { get; init; }
}
