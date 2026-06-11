using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Context;

public interface IClientContext
{
    Client? Client { get; }
    Client GetRequiredClient();
}

public class HttpClientContext : IClientContext
{
    private const string ClientItemKey = "client";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpClientContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Client? Client =>
        _httpContextAccessor.HttpContext?.Items["client"] as Client;

    public Client GetRequiredClient() =>
        Client ?? throw OAuthException.FromInvalidClient();

    public static void SetClient(HttpContext context, Client client)
    {
        context.Items[ClientItemKey] = client;
    }
}
