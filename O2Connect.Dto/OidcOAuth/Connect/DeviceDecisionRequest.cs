using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.OidcOAuth.Connect;

public sealed record DeviceDecisionRequest
{
    [FromForm(Name = "approved")]
    public required bool Approved { get; init; }
}
