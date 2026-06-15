using O2Connect.Api.Models.Store;
using System.Linq.Expressions;

namespace O2Connect.Api.Repositories.Filters;

public sealed record ClientFilter
{
    public static ClientFilter Empty => new();

    public string? Name { get; init; }
    public IReadOnlySet<EntityStatus>? Status { get; init; }
    public DateTimeOffset? MinCreatedAt { get; init; }
    public DateTimeOffset? MaxCreatedAt { get; init; }
    public DateTimeOffset? MinLastModifiedAt { get; init; }
    public DateTimeOffset? MaxLastModifiedAt { get; init; }
    public DateTimeOffset? MinRevokedAt { get; init; }
    public DateTimeOffset? MaxRevokedAt { get; init; }
    public IReadOnlySet<string>? GrantTypes { get; init; }
    public IReadOnlySet<string>? Scopes { get; init; }
    public IReadOnlySet<string>? AuthenticationMethods { get; init; }
    public IReadOnlySet<string>? ResponseTypes { get; init; }

    public bool IsEmpty =>
        Name is null &&
        Status is null &&
        MinCreatedAt is null &&
        MaxCreatedAt is null &&
        MinLastModifiedAt is null &&
        MaxLastModifiedAt is null &&
        MinRevokedAt is null &&
        MaxRevokedAt is null &&
        GrantTypes is null &&
        Scopes is null &&
        AuthenticationMethods is null &&
        ResponseTypes is null;

    public Expression<Func<Client, bool>> ToExpression()
    {
        return client =>
            (string.IsNullOrEmpty(Name) || 
                client.NormalizedName.Contains(Name)) &&
            (Status == null || Status.Contains(client.Status)) &&
            (MinCreatedAt == null || client.CreatedAt >= MinCreatedAt) &&
            (MaxCreatedAt == null || client.CreatedAt <= MaxCreatedAt) &&
            (MinLastModifiedAt == null || 
                (client.LastModifiedAt != null && client.LastModifiedAt >= MinLastModifiedAt)) &&
            (MaxLastModifiedAt == null || 
                (client.LastModifiedAt != null && client.LastModifiedAt <= MaxLastModifiedAt)) &&
            (MinRevokedAt == null || 
                (client.RevokedAt != null && client.RevokedAt >= MinRevokedAt)) &&
            (MaxRevokedAt == null || 
                (client.RevokedAt != null && client.RevokedAt <= MaxRevokedAt)) &&
            (GrantTypes == null || 
                GrantTypes.All(gt => client.AllowedGrantTypes.Contains(gt))) &&
            (Scopes == null || 
                Scopes.All(s => client.AllowedScopes.Contains(s))) &&
            (AuthenticationMethods == null || 
                AuthenticationMethods.All(am => client.AllowedAuthenticationMethods.Contains(am))) &&
            (ResponseTypes == null || 
                ResponseTypes.All(rt => client.AllowedResponseTypes.Contains(rt)));
    }
}
