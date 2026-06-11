using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.Requests;

public sealed record DeviceDecisionRequest
{
    [FromForm(Name = "approved")]
    public required bool Approved { get; init; }
}
