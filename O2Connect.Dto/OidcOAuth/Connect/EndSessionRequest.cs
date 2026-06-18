using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.OidcOAuth.Connect;

public sealed record EndSessionRequest
{
    [FromQuery(Name = "id_token_hint")]
    public string? IdTokenHint { get; init; }

    [FromQuery(Name = "post_logout_redirect_uri")]
    public string? PostLogoutRedirectUri { get; init; }

    [FromQuery(Name = "state")]
    public string? State { get; init; }
}
