using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Models.Mappers;

public static class ClientServiceModelsMapper
{
    public static ClientPagination ToPagination(this ClientsPaginationRequest request)
    {
        return new ClientPagination
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? "created_at",
            Order = request.Order ?? "desc"
        };
    }

    public static ClientPagination ToPagination(this ClientsSearchPaginationRequest request)
    {
        return new ClientPagination
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? "created_at",
            Order = request.Order ?? "desc"
        };
    }

    public static ClientsPaginationRequest ToPaginationRequest(this ClientsSearchPaginationRequest request)
    {
        return new ClientsPaginationRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy ?? "created_at",
            Order = request.Order ?? "desc"
        };
    }

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

    public static ClientSummaryDto ToSummary(this Client client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        var summary = new ClientSummaryDto
        {
            Id = client.Id,
            Name = client.Name,
            ImageUrl = client.ImageUrl,
            Status = client.Status.ToString(),
        };

        return summary;
    }

    public static ClientDetailsResponse ToDetails(this Client client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        var detail = new ClientDetailsResponse
        {
            Id = client.Id,
            Name = client.Name,
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
