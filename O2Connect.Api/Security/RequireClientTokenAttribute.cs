using Microsoft.AspNetCore.Authorization;

namespace O2Connect.Api.Security;

public sealed class RequireClientTokenAttribute : AuthorizeAttribute
{
    public const string PolicyName = "client_token";

    public RequireClientTokenAttribute()
    {
        Policy = PolicyName;
    }
}
