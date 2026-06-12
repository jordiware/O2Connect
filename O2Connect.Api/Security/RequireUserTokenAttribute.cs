using Microsoft.AspNetCore.Authorization;

namespace O2Connect.Api.Security;

public sealed class RequireUserTokenAttribute : AuthorizeAttribute
{
    public const string PolicyName = "user_token";

    public RequireUserTokenAttribute()
    {
        Policy = PolicyName;
    }
}
