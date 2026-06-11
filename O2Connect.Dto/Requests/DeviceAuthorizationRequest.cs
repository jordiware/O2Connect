using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.Requests;

public sealed record DeviceAuthorizationRequest
{
    [FromForm(Name = "client_id")]
    public required string ClientId { get; init; }

    [FromForm(Name = "scope")]
    public required string Scope { get; init; }
}
