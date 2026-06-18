using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Models.Mappers;

public static class ClientServiceModelsMapper
{
    public static ClientFilter ToFilter(this ClientsSearchFilterRequest filterRequest)
    {
        if (filterRequest == null)
            return ClientFilter.Empty;

        var filter = new ClientFilter
        {
            Name = filterRequest.Name,
            Status = filterRequest.Status?.Select(s => Enum.Parse<EntityStatus>(s, true)).ToHashSet(),
            MinCreatedAt = filterRequest.MinCreatedAt,
            MaxCreatedAt = filterRequest.MaxCreatedAt,
            MinLastModifiedAt = filterRequest.MinLastModifiedAt,
            MaxLastModifiedAt = filterRequest.MaxLastModifiedAt,
            MinRevokedAt = filterRequest.MinRevokedAt,
            MaxRevokedAt = filterRequest.MaxRevokedAt,
            GrantTypes = filterRequest.GrantTypes?.ToHashSet(StringComparer.Ordinal),
            Scopes = filterRequest.Scopes?.ToHashSet(StringComparer.Ordinal),
            AuthenticationMethods = filterRequest.AuthenticationMethods?.ToHashSet(StringComparer.Ordinal),
            ResponseTypes = filterRequest.ResponseTypes?.ToHashSet(StringComparer.Ordinal)
        };

        return filter;
    }

    public static ClientSummaryResponse ToSummaryDto(this Client client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        var summary = new ClientSummaryResponse
        {
            Id = client.Id,
            Name = client.DisplayName,
            ImageUrl = client.ImageUrl,
            Status = client.Status.ToString(),
        };

        return summary;
    }

    public static ClientDetailResponse ToDetailDto(this Client client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        var detail = new ClientDetailResponse
        {
            Id = client.Id,
            Name = client.DisplayName,
            ImageUrl = client.ImageUrl,
            Status = client.Status.ToString(),
            OwnerId = client.OwnerId,
            CreatedAt = client.CreatedAt,
            LastModifiedAt = client.LastModifiedAt,
            RevokedAt = client.RevokedAt,
            RedirectUris = client.RedirectUris.ToList(),
            AllowedGrantTypes = client.AllowedGrantTypes.ToList(),
            AllowedScopes = client.AllowedScopes.ToList(),
            AllowedAuthenticationMethods = client.AllowedAuthenticationMethods.ToList(),
            AllowedResponseTypes = client.AllowedResponseTypes.ToList()
        };

        return detail;
    }
}
