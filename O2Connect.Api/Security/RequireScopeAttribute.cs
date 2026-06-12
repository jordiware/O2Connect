using Microsoft.AspNetCore.Authorization;

namespace O2Connect.Api.Security;

public sealed class RequireScopeAttribute : AuthorizeAttribute
{
    public RequireScopeAttribute(params string[] scopes)
        : this(RequireScopeMode.All, scopes) { }

    public RequireScopeAttribute(RequireScopeMode mode, params string[] scopes)
    {
        Policy = BuildPolicyName(mode, scopes);
    }

    private static string BuildPolicyName(RequireScopeMode mode, string[] scopes)
    {
        var modePart = mode.ToString().ToLowerInvariant();
        var scopesPart = string.Join("|", scopes);

        return $"scope:{modePart}:{scopesPart}";
    }
}
