using System.Security.Claims;

namespace O2Connect.Api.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? ClientId { get; }
    bool IsAuthenticated { get; }

    ClaimsPrincipal Principal { get; }
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

    public string? ClientId =>
        Principal.FindFirst("client_id")?.Value;
}
