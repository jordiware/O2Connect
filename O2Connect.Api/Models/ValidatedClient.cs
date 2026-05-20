using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Models;

public sealed class ValidatedClient
{
    public required Client Client { get; set; }
    public required IReadOnlyCollection<string> RequestedScopes { get; set; }
}
