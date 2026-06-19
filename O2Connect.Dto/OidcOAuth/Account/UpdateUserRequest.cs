using System.Text.Json.Serialization;

namespace O2Connect.Dto.OidcOAuth.Account;

public sealed record UpdateUserRequest
{
    [JsonPropertyName("display_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DisplayName { get; init; }

    [JsonPropertyName("picture_uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PictureUri { get; init; }
}
