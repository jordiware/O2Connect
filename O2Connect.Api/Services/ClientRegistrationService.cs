using O2Connect.Api.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface IClientRegistrationService
{
    Task<ClientRegistrationResponse> HandleAsync(ClientRegistrationRequest request,
                                                 string ownerId,
                                                 CancellationToken ct);
}

public class ClientRegistrationService : IClientRegistrationService
{
    private static readonly HashSet<string> AllowedScopes =
    [
        "openid",
        "profile",
        "email",
        "api"
    ];

    private readonly IClientRepository _clientRepository;
    private readonly ISecretHasher _secretHasher;

    public ClientRegistrationService(
        IClientRepository clientRepository,
        ISecretHasher secretHasher)
    {
        _clientRepository = clientRepository;
        _secretHasher = secretHasher;
    }

    public async Task<ClientRegistrationResponse> HandleAsync(ClientRegistrationRequest request,
                                                              string ownerId,
                                                              CancellationToken ct)
    {
        var authMethod = request.TokenEndpointAuthMethod ?? "client_secret_basic";
        var clientId = Guid.NewGuid().ToString("N");
        var clientSecret = authMethod == "none" ? null : SecureCodeGenerator.GenerateBase64UrlToken();

        var client = BuildClient(request, ownerId, clientId, clientSecret);

        await _clientRepository.StoreAsync(client, ct);

        return new ClientRegistrationResponse
        {
            ClientId = clientId,
            ClientName = client.ClientName,
            ClientSecret = clientSecret,
            ClientIdIssuedAt = client.CreatedAt.ToUnixTimeSeconds(),
            ClientSecretExpiresAt = 0,
            RedirectUris = client.RedirectUris.ToArray(),
            GrantTypes = client.AllowedGrantTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            ResponseTypes = client.AllowedResponseTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            TokenEndpointAuthMethod = authMethod,
            Scope = string.Join(' ', client.AllowedScopes.OrderBy(x => x, StringComparer.Ordinal))
        };
    }

    private Client BuildClient(ClientRegistrationRequest request,
                               string ownerId,
                               string clientId,
                               string? clientSecret)
    {
        var now = DateTimeOffset.UtcNow;

        // 1. Defaults (SPEC + your policy)
        var grantTypes = request.GrantTypes ?? ["authorization_code"];
        var responseTypes = request.ResponseTypes
                            ?? (grantTypes.Contains("authorization_code") ? ["code"] : []);
        var authMethod = request.TokenEndpointAuthMethod ?? "client_secret_basic";

        // 2. Normalize scope (IMPORTANT: space-delimited → set)
        var scopes = request.Scope?
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>();

        if (scopes.Count == 0)
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        if (scopes.Count > 20)
            throw OAuthException.FromInvalidRequest("invalid_scope");

        foreach (var scope in scopes)
        {
            if (!AllowedScopes.Contains(scope))
                throw OAuthException.FromInvalidRequest("invalid_scope");
        }

        if (scopes.Contains("openid") && !grantTypes.Contains("authorization_code"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        if (scopes.Contains("openid") && !responseTypes.Contains("code"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        // 3. Validate / restrict (VERY IMPORTANT)
        ValidateGrantTypes(grantTypes);

        if (responseTypes.Length > 0)
            ValidateResponseTypes(responseTypes);
        
        ValidateAuthMethod(authMethod);
        
        ValidateGrantResponseConsistency(grantTypes, responseTypes);

        // 4. Redirect URIs → set
        if (request.RedirectUris == null || request.RedirectUris.Length == 0)
        {
            if (grantTypes.Contains("authorization_code"))
                throw OAuthException.FromInvalidRequest("invalid_client_metadata");
        }

        var redirectUris = request.RedirectUris!.Select(NormalizeUri)
                                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (redirectUris.Count == 0 && grantTypes.Contains("authorization_code"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        // 5. Secret policy
        var requiresSecret = authMethod != "none";

        if (!requiresSecret && grantTypes.Contains("client_credentials"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        // 6. Client name fallback (REQUIRED in your model)
        var clientName = request.ClientName ?? $"client-{Guid.NewGuid():N}";

        var hashedSecret = default(string?);
        if (!string.IsNullOrWhiteSpace(clientSecret))
            _secretHasher.TryHash(clientSecret, out hashedSecret);

        return new Client
        {
            ClientId = clientId,
            ClientName = clientName,
            CreatedAt = now,
            OwnerId = ownerId,

            ClientSecret = hashedSecret,
            RequiresSecret = requiresSecret,

            RedirectUris = redirectUris,

            AllowedGrantTypes = grantTypes.OrderBy(x => x, StringComparer.Ordinal)
                                          .ToHashSet(StringComparer.Ordinal),
            AllowedResponseTypes = responseTypes.OrderBy(x => x, StringComparer.Ordinal)
                                                .ToHashSet(StringComparer.Ordinal),
            AllowedAuthenticationMethods = new HashSet<string> { authMethod },

            AllowedScopes = scopes,

            // Defaults (your domain policy)
            RequiresPkce = true,
            RequiresConsent = true,
            AllowPlainPkce = false,
            AllowPar = true
        };
    }

    private static string NormalizeUri(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            throw OAuthException.FromInvalidRedirectUri("invalid_redirect_uri");

        if (!string.IsNullOrWhiteSpace(parsed.Query) && parsed.Query.Contains("redirect_uri"))
            throw OAuthException.FromInvalidRedirectUri("invalid_redirect_uri");

        if (!string.IsNullOrWhiteSpace(parsed.Fragment))
            throw OAuthException.FromInvalidRedirectUri("invalid_redirect_uri");

        if (parsed.Scheme != Uri.UriSchemeHttps 
            && !(parsed.Host == "localhost" && parsed.Scheme == Uri.UriSchemeHttp))
            throw OAuthException.FromInvalidRedirectUri("invalid_redirect_uri");

        return parsed.ToString();
    }

    private static void ValidateGrantTypes(IEnumerable<string> grantTypes)
    {
        if (!grantTypes.All(g => GrantType.TryParse(g, out _)))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");
    }

    private static void ValidateResponseTypes(IEnumerable<string> responseTypes)
    {
        foreach (var rt in responseTypes)
        {
            if (rt != "code")
                throw OAuthException.FromInvalidRequest("invalid_client_metadata");
        }
    }

    private static void ValidateAuthMethod(string? authMethod)
    {
        if (string.IsNullOrWhiteSpace(authMethod))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        if (!ClientAuthenticationMethod.TryParse(authMethod, out _))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");
    }

    private static void ValidateGrantResponseConsistency(IReadOnlyCollection<string> grantTypes,
                                                         IReadOnlyCollection<string> responseTypes)
    {
        if (responseTypes.Count == 0 && !grantTypes.Contains("client_credentials"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        if (grantTypes.Contains("client_credentials") && responseTypes.Count > 0)
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        var grants = grantTypes.ToHashSet(StringComparer.Ordinal);
        var responses = responseTypes.ToHashSet(StringComparer.Ordinal);

        if (grants.Contains("authorization_code") && !responses.Contains("code"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");
    }
}
