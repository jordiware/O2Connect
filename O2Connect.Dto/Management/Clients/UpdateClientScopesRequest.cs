using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Clients;

public sealed record UpdateClientScopesRequest
{
    [JsonPropertyName("scopes")]
    public required IReadOnlyList<string> Scopes { get; init; }
}
