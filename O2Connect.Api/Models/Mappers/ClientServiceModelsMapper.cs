using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories.Filters;
using O2Connect.Dto.Management.Clients;

namespace O2Connect.Api.Models.Mappers;

public static class ClientServiceModelsMapper
{
    public static ClientFilter ToFilter(this ClientSearchRequest searchRequest)
    {
        if (searchRequest == null)
            return ClientFilter.Empty;

        var filter = new ClientFilter
        {
            Name = searchRequest.Name,
            Status = searchRequest.Status?.Select(s => Enum.Parse<EntityStatus>(s, true)).ToHashSet(),
            MinCreatedAt = searchRequest.MinCreatedAt,
            MaxCreatedAt = searchRequest.MaxCreatedAt,
            MinLastModifiedAt = searchRequest.MinLastModifiedAt,
            MaxLastModifiedAt = searchRequest.MaxLastModifiedAt,
            MinRevokedAt = searchRequest.MinRevokedAt,
            MaxRevokedAt = searchRequest.MaxRevokedAt,
            GrantTypes = searchRequest.GrantTypes?.ToHashSet(StringComparer.Ordinal),
            Scopes = searchRequest.Scopes?.ToHashSet(StringComparer.Ordinal),
            AuthenticationMethods = searchRequest.AuthenticationMethods?.ToHashSet(StringComparer.Ordinal),
            ResponseTypes = searchRequest.ResponseTypes?.ToHashSet(StringComparer.Ordinal)
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
