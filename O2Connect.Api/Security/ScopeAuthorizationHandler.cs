using Microsoft.AspNetCore.Authorization;

namespace O2Connect.Api.Security;

public sealed class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   ScopeRequirement requirement)
    {
        var scopeClaims = context.User.FindAll("scope");

        if (!scopeClaims.Any())
            return Task.CompletedTask;

        var userScopes = scopeClaims
            .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToHashSet(StringComparer.Ordinal);

        var success = requirement.Mode switch
        {
            RequireScopeMode.All => requirement.Scopes.All(userScopes.Contains),
            RequireScopeMode.Any => requirement.Scopes.Any(userScopes.Contains),
            _ => false
        };

        if (success)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
