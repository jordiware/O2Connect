using Microsoft.AspNetCore.Authorization;

namespace O2Connect.Api.Security;

public sealed class ScopeRequirement : IAuthorizationRequirement
{
    public IReadOnlyCollection<string> Scopes { get; }
    public RequireScopeMode Mode { get; }

    public ScopeRequirement(IEnumerable<string> scopes, RequireScopeMode mode)
    {
        Scopes = scopes.ToArray();
        Mode = mode;
    }
}
