using System.Text.Json.Serialization;

namespace O2Connect.Dto.Requests;

public sealed record ConsentDecisionRequest
{
    [JsonPropertyName("approved")]
    public bool Approved { get; init; }

    [JsonPropertyName("approved_scopes")]
    public required string ApprovedScopes { get; init; }
}
