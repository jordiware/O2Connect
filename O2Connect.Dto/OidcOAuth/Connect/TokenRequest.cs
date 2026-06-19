using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.OidcOAuth.Connect;

public sealed record TokenRequest
{
    [FromForm(Name = "grant_type")]
    public required string GrantType { get; init; }

    [FromForm(Name = "client_id")]
    public string? ClientId { get; init; }

    [FromForm(Name = "client_secret")]
    public string? ClientSecret { get; init; }

    [FromForm(Name = "client_assertion")]
    public string? ClientAssertion { get; init; }

    [FromForm(Name = "client_assertion_type")]
    public string? ClientAssertionType { get; init; }

    [FromForm(Name = "code")]
    public string? Code { get; init; }

    [FromForm(Name = "device_code")]
    public string? DeviceCode { get; init; }

    [FromForm(Name = "redirect_uri")]
    public string? RedirectUri { get; init; }

    [FromForm(Name = "code_verifier")]
    public string? CodeVerifier { get; init; }

    [FromForm(Name = "refresh_token")]
    public string? RefreshToken { get; init; }

    [FromForm(Name = "scope")]
    public string? Scope { get; init; }

    [FromForm(Name = "resource")]
    public string? Resource { get; init; }

    [FromForm(Name = "audience")]
    public string? Audience { get; init; }
}
