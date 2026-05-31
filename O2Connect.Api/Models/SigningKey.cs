using Microsoft.IdentityModel.Tokens;

namespace O2Connect.Api.Models;

public sealed class SigningKey
{
    public required string KeyId { get; init; }
    public required SecurityKey Key { get; init; }
    public required SigningCredentials Credentials { get; init; }
}
