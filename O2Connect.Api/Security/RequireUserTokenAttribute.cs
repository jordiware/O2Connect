using Microsoft.AspNetCore.Authorization;

namespace O2Connect.Api.Security;

public sealed class RequireUserTokenAttribute : AuthorizeAttribute
{
    public RequireUserTokenAttribute()
    {
        Policy = "user_token";
    }
}
