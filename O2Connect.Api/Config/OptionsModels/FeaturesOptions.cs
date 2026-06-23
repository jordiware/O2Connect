namespace O2Connect.Api.Config.OptionsModels;

public sealed record FeaturesOptions
{
    public const string SectionName = "Features";

    public bool EnableDynamicClientRegistration { get; init; }
    public bool EnableDeviceFlow { get; init; }
    public bool EnableRevocationEndpoint { get; init; }
}
