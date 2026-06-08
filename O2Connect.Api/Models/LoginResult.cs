using O2Connect.Dto.Responses;

namespace O2Connect.Api.Models;

public abstract record LoginResult;

public sealed record LoginBadRequest(string Message) : LoginResult;
public sealed record LoginUnauthorized(string Message) : LoginResult;
public sealed record LoginForbidden : LoginResult;

public sealed record LoginRedirect(RedirectResponse RedirectResponse) : LoginResult;

public sealed record LoginTokenSuccess(TokenResponse TokenResponse) : LoginResult;
