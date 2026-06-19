using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Clients;

public sealed record UpdateClientRedirectUrisRequest
{
    [JsonPropertyName("redirect_uris")]
    public required IReadOnlyList<string> RedirectUris { get; init; }
}
