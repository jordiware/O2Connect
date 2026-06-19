using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Clients;

public sealed record UpdateClientDisplayNameRequest
{
    [JsonPropertyName("display_name")]
    public required string DisplayName { get; init; }
}
