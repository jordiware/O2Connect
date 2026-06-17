using O2Connect.Api.Models.SmartEnums;
using System.Security.Claims;

namespace O2Connect.Api.Services;

public interface ICurrentUserService
{
    ClaimsPrincipal Principal { get; }

    string? UserId { get; }
    string? ClientId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }

    bool HasScope(string scope);
    bool HasRole(UserRole role);
}

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal Principal =>
        _httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal(new ClaimsIdentity());

    public bool IsAuthenticated =>
        Principal.Identity?.IsAuthenticated ?? false;

    public string? UserId =>
        Principal.FindFirst("sub")?.Value;

    public IReadOnlyCollection<string> Roles =>
        Principal.FindAll("role")
                 .Select(x => x.Value)
                 .ToArray();

    public string? ClientId =>
        Principal.FindFirst("client_id")?.Value;

    public bool HasScope(string scope)
    {
        return Principal.FindFirst("scope")?.Value?.Split(' ')
                        .Contains(scope, StringComparer.Ordinal) == true;
    }

    public bool HasRole(UserRole role)
    {
        return Principal.FindAll("role")
                        .Union(Principal.FindAll("roles"))
                        .SelectMany(x => x.Value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        .Select(x => x.TrimStart(['[', '{']).TrimEnd([']', '}']).Trim())
                        .Any(x => x.Equals(role, StringComparison.Ordinal));
    }
}
