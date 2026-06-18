using O2Connect.Api.Models.Store;
using System.Linq.Expressions;

namespace O2Connect.Api.Repositories.Filters;

public sealed record UserFilter
{
    public static UserFilter Empty => new();

    public string? Name { get; init; }
    public string? Email { get; init; }
    public IReadOnlySet<EntityStatus>? Status { get; init; }
    public DateTimeOffset? MinCreatedAt { get; init; }
    public DateTimeOffset? MaxCreatedAt { get; init; }
    public DateTimeOffset? MinLastModifiedAt { get; init; }
    public DateTimeOffset? MaxLastModifiedAt { get; init; }
    public DateTimeOffset? MinRevokedAt { get; init; }
    public DateTimeOffset? MaxRevokedAt { get; init; }

    public bool IsEmpty =>
        Name is null &&
        Email is null &&
        Status is null &&
        MinCreatedAt is null &&
        MaxCreatedAt is null &&
        MinLastModifiedAt is null &&
        MaxLastModifiedAt is null &&
        MinRevokedAt is null &&
        MaxRevokedAt is null;

    public Expression<Func<User, bool>> ToExpression()
    {
        return client =>
            (string.IsNullOrWhiteSpace(Name) || client.NormalizedUsername.Contains(Name) 
                || (!string.IsNullOrWhiteSpace(client.DisplayName) && client.DisplayName.Contains(Name))) &&
            (string.IsNullOrWhiteSpace(Email) || client.Email.Contains(Email)) &&
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
                (client.RevokedAt != null && client.RevokedAt <= MaxRevokedAt));
    }
}
