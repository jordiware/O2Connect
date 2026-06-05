using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.Requests;

public sealed record AuthorizationRequest
{
    [FromQuery(Name = "response_type")]
    public required string ResponseType { get; init; }

    [FromQuery(Name = "client_id")]
    public required string ClientId { get; init; }

    [FromQuery(Name = "redirect_uri")]
    public required string RedirectUri { get; init; }

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

    [FromQuery(Name = "response_mode")]
    public string? ResponseMode { get; init; }
}
