using System.Text.Json.Serialization;

namespace O2Connect.Dto.Management.Users;

public sealed record UpdateUserRoleRequest
{
    [JsonPropertyName("role")]
    public required string Role { get; init; }
}
