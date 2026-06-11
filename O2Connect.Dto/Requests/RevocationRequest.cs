using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.Requests;

public sealed record RevocationRequest
{
    [FromForm(Name = "token")]
    public required string Token { get; init; }

    [FromForm(Name = "token_type_hint")]
    public string? TokenTypeHint { get; init; }
}
