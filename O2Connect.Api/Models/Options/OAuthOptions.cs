namespace O2Connect.Api.Models.Options;

public sealed record OAuthOptions
{
    public required string TokenEndpoint { get; init; }
}
