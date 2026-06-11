namespace O2Connect.Api.Models.Options;

public sealed record OAuthOptions
{
    public const string SectionName = "OAuth";

    public required string TokenEndpoint { get; init; }
}
