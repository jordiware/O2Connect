using System.Security.Claims;

namespace O2Connect.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static bool HasScope(this ClaimsPrincipal user, string scope)
    {
        return user.FindAll("scope")
                   .Concat(user.FindAll("scp"))
                   .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                   .Any(s => s.Equals(scope, StringComparison.Ordinal));
    }

    public static bool IsClientToken(this ClaimsPrincipal user)
    {
        return user.HasClaim(c => c.Type == "client_id") && !user.HasClaim(c => c.Type == "sub");
    }

    public static bool IsUserToken(this ClaimsPrincipal user)
    {
        return user.HasClaim(c => c.Type == "sub");
    }
}
