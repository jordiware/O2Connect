using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.Requests;

public sealed record AuthorizationRequest
{
    [FromQuery(Name = "response_type")]
    public string ResponseType { get; init; } = default!;

    [FromQuery(Name = "response_mode")]
    public string ResponseMode { get; init; } = "query";

    [FromQuery(Name = "client_id")]
    public string ClientId { get; init; } = default!;

    [FromQuery(Name = "redirect_uri")]
    public string RedirectUri { get; init; } = default!;

    [FromQuery(Name = "scope")]
    public string? Scope { get; init; }

    [FromQuery(Name = "state")]
    public string? State { get; init; }

    [FromQuery(Name = "nonce")]
    public string? Nonce { get; init; }

    [FromQuery(Name = "code_challenge")]
    public string? CodeChallenge { get; init; }

    [FromQuery(Name = "code_challenge_method")]
    public string? CodeChallengeMethod { get; init; }
}
