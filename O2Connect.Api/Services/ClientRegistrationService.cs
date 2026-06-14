using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;
using System.Globalization;
using System.Text;

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

    public ClientRegistrationService(
        IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientRegistrationResponse> HandleAsync(ClientRegistrationRequest request,
                                                              string ownerId,
                                                              CancellationToken ct)
    {
        var method = request.TokenEndpointAuthMethod ?? "none";

        if (!string.Equals(method, "none", StringComparison.Ordinal))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        var clientId = Guid.NewGuid().ToString("N");

        var client = BuildClient(request, ownerId, clientId);

        await _clientRepository.StoreAsync(client, ct);

        return new ClientRegistrationResponse
        {
            ClientId = clientId,
            ClientName = client.Name,
            ClientSecret = null,
            ClientIdIssuedAt = client.CreatedAt.ToUnixTimeSeconds(),
            ClientSecretExpiresAt = 0,
            RedirectUris = client.RedirectUris.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            GrantTypes = client.AllowedGrantTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            ResponseTypes = client.AllowedResponseTypes.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            TokenEndpointAuthMethod = method,
            Scope = string.Join(' ', client.AllowedScopes)
        };
    }

    private Client BuildClient(ClientRegistrationRequest request, string ownerId, string clientId)
    {
        var now = DateTimeOffset.UtcNow;

        // Defaults (SPEC + your policy)
        var grantTypes = request.GrantTypes ?? ["authorization_code"];
        var responseTypes = request.ResponseTypes
                            ?? (grantTypes.Contains("authorization_code") ? ["code"] : []);

        // Normalize scope (IMPORTANT: space-delimited → set)
        var scopes = ValueSet.FromDataString(request.Scope, ' ').Values;

        if (scopes.Count == 0)
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        if (scopes.Count > 20)
            throw OAuthException.FromInvalidRequest("invalid_scope");

        if (!scopes.All(AllowedScopes.Contains))
            throw OAuthException.FromInvalidRequest("invalid_scope");

        if (scopes.Contains("openid") && !grantTypes.Contains("authorization_code"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        if (scopes.Contains("openid") && !responseTypes.Contains("code"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        // Validate / restrict
        ValidateGrantTypes(grantTypes);

        ValidateResponseTypes(responseTypes);

        // Redirect URIs → set
        var redirectUris = (request.RedirectUris ?? []).Select(NormalizeUri)
                                                       .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (redirectUris.Count == 0 && grantTypes.Contains("authorization_code"))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        // Client name fallback (REQUIRED in your model)
        var clientName = request.ClientName ?? $"client-{Guid.NewGuid():N}";

        return new Client
        {
            Id = clientId,
            Name = clientName,
            NormalizedName = NormalizeName(clientName),
            Status = EntityStatus.Active,
            OwnerId = ownerId,
            CreatedAt = now,

            ClientSecret = null,
            RequiresSecret = false,

            RedirectUris = redirectUris,

            AllowedGrantTypes = grantTypes.ToHashSet(StringComparer.Ordinal),
            AllowedResponseTypes = responseTypes.ToHashSet(StringComparer.Ordinal),
            AllowedAuthenticationMethods = new HashSet<string> { "none" },

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

        if (!string.IsNullOrWhiteSpace(parsed.Fragment))
            throw OAuthException.FromInvalidRedirectUri("invalid_redirect_uri");

        if (parsed.Scheme != Uri.UriSchemeHttps
            && !(parsed.Host == "localhost" && parsed.Scheme == Uri.UriSchemeHttp))
            throw OAuthException.FromInvalidRedirectUri("invalid_redirect_uri");

        return parsed.GetLeftPart(UriPartial.Path);
    }

    private static void ValidateGrantTypes(IEnumerable<string> grantTypes)
    {
        if (!grantTypes.All(g => string.Equals(g, GrantType.AuthorizationCode.Value, StringComparison.Ordinal)))
            throw OAuthException.FromInvalidRequest("unsupported_grant_type");
    }

    private static void ValidateResponseTypes(IEnumerable<string> responseTypes)
    {
        if (!responseTypes.Any())
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");

        if (!responseTypes.All(rt => string.Equals(rt, "code", StringComparison.Ordinal)))
            throw OAuthException.FromInvalidRequest("invalid_client_metadata");
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw OAuthException.FromInvalidRequest();

        var trimmed = name.Trim();

        var normalized = trimmed.Normalize(NormalizationForm.FormKD);

        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString()
                 .Normalize(NormalizationForm.FormKC)
                 .ToLowerInvariant();
    }
}
