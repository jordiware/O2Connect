namespace O2Connect.Api.Context;

public interface IUserContext
{
    bool IsAuthenticated { get; }
}

public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
