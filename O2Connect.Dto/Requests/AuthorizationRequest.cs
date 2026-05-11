using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.RequestDtos;

public class AuthorizationRequest
{
    [FromQuery(Name = "response_type")]
    public string ResponseType { get; set; } = default!;

    [FromQuery(Name = "client_id")]
    public string ClientId { get; set; } = default!;

    [FromQuery(Name = "redirect_uri")]
    public string RedirectUri { get; set; } = default!;

    [FromQuery(Name = "scope")]
    public string? Scope { get; set; }

    [FromQuery(Name = "state")]
    public string? State { get; set; }

    [FromQuery(Name = "nonce")]
    public string? Nonce { get; set; }

    [FromQuery(Name = "code_challenge")]
    public string? CodeChallenge { get; set; }

    [FromQuery(Name = "code_challenge_method")]
    public string? CodeChallengeMethod { get; set; }
}
