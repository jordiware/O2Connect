namespace O2Connect.Api.Models;

public sealed class ValidatedClient
{
    public required Client Client { get; init; }
    public required IReadOnlyCollection<string> RequestedScopes { get; init; }
}
