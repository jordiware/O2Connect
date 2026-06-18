using O2Connect.Dto.OidcOAuth;
using O2Connect.Dto.OidcOAuth.Connect;

namespace O2Connect.Api.Models;

public abstract record LoginResult;

public sealed record LoginRedirect(RedirectResponse RedirectResponse) : LoginResult;

public sealed record LoginTokenSuccess(TokenResponse TokenResponse) : LoginResult;
