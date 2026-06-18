using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Mappers;
using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Clients;
using System.Globalization;
using System.Text;

namespace O2Connect.Api.Services.Management;

public interface IManagementClientsService
{
    Task<ClientDetailResponse?> GetClientAsync(string clientId,
                                               CancellationToken ct);
    Task<ClientListResponse> QueryClientsAsync(EntityPagination pagination,
                                               ClientFilter filter,
                                               CancellationToken ct);
    Task UpdateClientDisplayNameAsync(string clientId,
                                      string displayName,
                                      CancellationToken ct);
    Task UpdateClientImageUrlAsync(string clientId,
                                   string imageUrl,
                                   CancellationToken ct);
    Task UpdateClientRedirectUrisAsync(string clientId,
                                       IReadOnlyList<string> redirectUris,
                                       CancellationToken ct);
    Task UpdateClientScopesAsync(string clientId,
                                 IReadOnlyList<string> scopes,
                                 CancellationToken ct);
    Task UpdateClientStatusAsync(string clientId,
                                 string status,
                                 CancellationToken ct);
}

public class ManagementClientsService : IManagementClientsService
{
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IUserConsentRepository _userConsentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ManagementClientsService> _logger;

    public ManagementClientsService(
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuthorizationCodeRepository authorizationCodeRepository,
        IUserConsentRepository userConsentRepository,
        ICurrentUserService currentUserService,
        ILogger<ManagementClientsService> logger)
    {
        _clientRepository = clientRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _userConsentRepository = userConsentRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ClientDetailResponse?> GetClientAsync(string clientId, CancellationToken ct)
    {
        var client = await _clientRepository.GetAsync(clientId, ct);

        if (client is null)
        {
            _logger.LogWarning("Client with ID {ClientId} not found.", clientId);
            return null;
        }

        var response = client.ToDetailDto();

        return response;
    }

    public async Task<ClientListResponse> QueryClientsAsync(EntityPagination pagination,
                                                            ClientFilter filter,
                                                            CancellationToken ct)
    {
        if (_currentUserService.HasRole(UserRole.Developer))
            filter = filter with { OwnerId = _currentUserService.UserId };

        var totalClients = await _clientRepository.CountAsync(filter, ct);

        if (totalClients == 0)
        {
            return new ClientListResponse
            {
                Items = [],
                TotalItems = 0,
                Page = pagination.Page,
                TotalPages = 0,
                PageSize = pagination.PageSize
            };
        }

        var pages = (int)Math.Ceiling((double)totalClients / pagination.PageSize);

        if (pagination.Page > pages)
        {
            _logger.LogWarning("Requested page {Page} is out of range. Total pages: {Pages}", pagination.Page, pages);
            throw ApiException.BadRequest("invalid_requested_page", $"Page must be between 1 and {pages}");
        }

        var skip = (pagination.Page - 1) * pagination.PageSize;
        var remainingItems = totalClients - skip;
        var pageSize = Math.Min(pagination.PageSize, remainingItems);

        pagination = pagination with { PageSize = pageSize };

        var clients = await _clientRepository.QueryAsync(pagination, filter, ct);

        var summaries = clients.Select(c => c.ToSummaryDto()).ToList();

        var response = new ClientListResponse
        {
            Items = summaries,
            TotalItems = totalClients,
            Page = pagination.Page,
            TotalPages = pages,
            PageSize = pagination.PageSize
        };

        return response;
    }

    public async Task UpdateClientDisplayNameAsync(string clientId,
                                                   string displayName,
                                                   CancellationToken ct)
    {
        displayName = displayName.Trim();
        var normalizedName = NormalizeName(displayName);
        var filter = new ClientFilter
        {
            Name = normalizedName
        };

        var clientCount = await _clientRepository.CountAsync(filter, ct);

        if (clientCount > 0)
        {
            _logger.LogWarning("There already exists a client with name {displayName}.", displayName);
            throw ApiException.BadRequest("invalid_redirect_uris",
                                          $"There already exists a client with name {displayName}.");
        }

        var client = await GetClientForUpdateAsync(clientId, ct);

        if (string.Equals(client.DisplayName, displayName, StringComparison.InvariantCulture))
        {
            _logger.LogInformation("Client {clientId} already has name {displayName}. No update needed.",
                                   client.Id,
                                   displayName);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        client = client with
        {
            DisplayName = displayName,
            NormalizedName = normalizedName,
            LastModifiedAt = now
        };

        await _clientRepository.StoreAsync(client, ct);
    }

    public async Task UpdateClientImageUrlAsync(string clientId,
                                                string imageUrl,
                                                CancellationToken ct)
    {
        imageUrl = imageUrl.Trim();

        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) || 
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            _logger.LogWarning("Invalid image URL scheme for client {ClientId}.", clientId);
            throw ApiException.BadRequest("invalid_image_url", "Image URL must be HTTP or HTTPS.");
        }

        if (uri.IsLoopback || string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Invalid image URL host for client {ClientId}.", clientId);
            throw ApiException.BadRequest("invalid_image_url", "Local URLs are not allowed.");
        }

        var client = await GetClientForUpdateAsync(clientId, ct);

        if (string.Equals(client.ImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Client already has image URL {imageUrl}. No update needed.",
                                   imageUrl);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        client = client with
        {
            ImageUrl = imageUrl,
            LastModifiedAt = now
        };

        await _clientRepository.StoreAsync(client, ct);
    }

    public async Task UpdateClientRedirectUrisAsync(string clientId,
                                                    IReadOnlyList<string> redirectUris,
                                                    CancellationToken ct)
    {
        var newRedirectUris = redirectUris.Select(r => r.Trim())
                                          .Where(r => !string.IsNullOrWhiteSpace(r))
                                          .Distinct()
                                          .ToHashSet();

        if (!newRedirectUris.All(r => Uri.IsWellFormedUriString(r, UriKind.Absolute)))
        {
            _logger.LogWarning("Invalid redirect URIs provided for client {ClientId}.", clientId);
            throw ApiException.BadRequest("invalid_redirect_uris", "One or more redirect URIs are invalid.");
        }

        var client = await GetClientForUpdateAsync(clientId, ct);

        if (client.RedirectUris.SetEquals(newRedirectUris))
            return;

        var now = DateTimeOffset.UtcNow;

        client = client with
        {
            RedirectUris = newRedirectUris,
            LastModifiedAt = now
        };

        await _clientRepository.StoreAsync(client, ct);
    }

    public async Task UpdateClientScopesAsync(string clientId,
                                              IReadOnlyList<string> scopes,
                                              CancellationToken ct)
    {
        var newScopes = scopes.Select(s => s.Trim())
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .Distinct()
                              .ToHashSet();

        if (!newScopes.All(Scopes.All.Contains))
        {
            var invalidScopes = newScopes.Except(Scopes.All);

            _logger.LogWarning("Invalid scopes provided: {InvalidScopes}", string.Join(", ", invalidScopes));

            throw ApiException.BadRequest("invalid_requested_scopes",
                                          $"Invalid scopes provided: {string.Join(", ", invalidScopes)}");
        }

        var client = await GetClientForUpdateAsync(clientId, ct);

        if (client.AllowedScopes.SetEquals(newScopes))
            return;

        var now = DateTimeOffset.UtcNow;

        client = client with
        {
            AllowedScopes = newScopes,
            LastModifiedAt = now
        };

        await _clientRepository.StoreAsync(client, ct);

        await _refreshTokenRepository.RevokeClientAsync(client.Id, ct);

        await _userConsentRepository.RevokeForClientAsync(client.Id, ct);
    }

    public async Task UpdateClientStatusAsync(string clientId, string status, CancellationToken ct)
    {
        status = status.Trim();

        if (!Enum.TryParse<EntityStatus>(status, true, out var newStatus))
        {
            _logger.LogWarning("Invalid status value: {Status}", status);
            throw ApiException.BadRequest("invalid_requested_status", $"Invalid status value: {status}");
        }

        var client = await GetClientForUpdateAsync(clientId, ct);

        if (newStatus == EntityStatus.Pending)
        {
            _logger.LogWarning("Client {ClientName} can't be reverted to pending status.", client.DisplayName);
            throw ApiException.BadRequest("invalid_requested_status",
                                          $"{client.DisplayName} can't revert to 'pending' status");
        }

        if (client.Status == newStatus)
            return;

        var now = DateTimeOffset.UtcNow;

        client = client with
        {
            Status = newStatus,
            LastModifiedAt = now
        };

        if (newStatus == EntityStatus.Revoked)
            client = client with { RevokedAt = now };

        await _clientRepository.StoreAsync(client, ct);

        if (client.Status != EntityStatus.Active)
        {
            await _refreshTokenRepository.RevokeClientAsync(client.Id, ct);
            await _authorizationCodeRepository.RevokeForClientAsync(client.Id, ct);
        }

        if (client.Status == EntityStatus.Revoked)
        {
            await _userConsentRepository.RevokeForClientAsync(client.Id, ct);
        }
    }

    private async Task<Client> GetClientForUpdateAsync(string clientId, CancellationToken ct)
    {
        var client = await _clientRepository.GetAsync(clientId, ct);

        if (client is null)
        {
            _logger.LogInformation("Client with ID '{ClientId}' not found.", clientId);
            throw ApiException.NotFound("client_not_found", $"Client '{clientId}' not found.");
        }

        if (client.Status == EntityStatus.Revoked)
        {
            _logger.LogInformation("Client '{ClientId}' is already revoked. No updates performed.", clientId);
            throw ApiException.Conflict("client_revoked",
                                        $"Client '{clientId}' is revoked and updates cannot be performed.");
        }

        if (!_currentUserService.HasRole(UserRole.Admin)
            && !(_currentUserService.HasRole(UserRole.Developer)
                && client.OwnerId.Equals(_currentUserService.UserId, StringComparison.Ordinal)))
        {
            _logger.LogWarning("Unprivileged user {UserId} accessing to client {ClientId}",
                               _currentUserService.UserId,
                               client.Id);
            throw ApiException.Unauthorized("restricted_access",
                                            "You don't have permissions for this action.");
        }

        return client;
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
