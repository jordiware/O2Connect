using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Clients;

public sealed record UpdateClientStatusRequest
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }
}
