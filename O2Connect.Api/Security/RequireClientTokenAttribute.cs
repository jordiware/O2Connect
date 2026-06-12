using Microsoft.AspNetCore.Authorization;

namespace O2Connect.Api.Security;

public sealed class RequireClientTokenAttribute : AuthorizeAttribute
{
    public RequireClientTokenAttribute()
    {
        Policy = "client_token";
    }
}
