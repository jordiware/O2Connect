using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace O2Connect.Api.Security;

public sealed class ScopePolicyProvider : DefaultAuthorizationPolicyProvider
{
    public ScopePolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("scope:", StringComparison.Ordinal))
        {
            var parts = policyName.Split(':', 3);

            if (parts.Length != 3)
                return Task.FromResult<AuthorizationPolicy?>(null);

            var modePart = parts[1];
            var scopesPart = parts[2];

            var mode = modePart switch
            {
                "all" => RequireScopeMode.All,
                "any" => RequireScopeMode.Any,
                _ => RequireScopeMode.All
            };

            var scopes = scopesPart.Split('|', StringSplitOptions.RemoveEmptyEntries);

            if (scopes.Length == 0)
                return Task.FromResult<AuthorizationPolicy?>(null);

            var policy = new AuthorizationPolicyBuilder().AddRequirements(new ScopeRequirement(scopes, mode))
                                                         .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return base.GetPolicyAsync(policyName);
    }
}
