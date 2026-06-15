using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Repositories.Filters;

public sealed record ClientSearchFilter
{
    public static ClientSearchFilter Empty => new();

    public string? Name { get; init; }
    public IReadOnlySet<EntityStatus>? Status { get; init; }
    public DateOnly? MinCreatedAt { get; init; }
    public DateOnly? MaxCreatedAt { get; init; }
    public DateOnly? MinLastModifiedAt { get; init; }
    public DateOnly? MaxLastModifiedAt { get; init; }
    public DateOnly? MinRevokedAt { get; init; }
    public DateOnly? MaxRevokedAt { get; init; }
    public IReadOnlySet<string>? GrantTypes { get; init; }
    public IReadOnlySet<string>? Scopes { get; init; }
    public IReadOnlySet<string>? AuthenticationMethods { get; init; }
    public IReadOnlySet<string>? ResponseTypes { get; init; }

    public bool Filter(Client client)
    {
        if (!string.IsNullOrWhiteSpace(Name) 
            && !client.NormalizedName.Contains(Name, StringComparison.InvariantCultureIgnoreCase))
            return false;

        if (Status != null && !Status.Contains(client.Status))
            return false;

        if (MinCreatedAt.HasValue
            && client.CreatedAt < MinCreatedAt.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
            return false;

        if (MaxCreatedAt.HasValue
            && client.CreatedAt > MaxCreatedAt.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
            return false;

        if (MinLastModifiedAt.HasValue
            && client.CreatedAt < MinLastModifiedAt.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
            return false;

        if (MaxLastModifiedAt.HasValue
            && client.CreatedAt > MaxLastModifiedAt.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
            return false;

        if (MinRevokedAt.HasValue
            && client.CreatedAt < MinRevokedAt.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
            return false;

        if (MinRevokedAt.HasValue
            && client.CreatedAt > MinRevokedAt.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
            return false;

        if (GrantTypes != null && !GrantTypes.IsSubsetOf(client.AllowedGrantTypes))
            return false;

        if (Scopes != null && !Scopes.IsSubsetOf(client.AllowedScopes))
            return false;

        if (AuthenticationMethods != null 
            && !AuthenticationMethods.IsSubsetOf(client.AllowedAuthenticationMethods))
            return false;

        if (ResponseTypes != null && !ResponseTypes.IsSubsetOf(client.AllowedResponseTypes))
            return false;

        return true;
    }
}
