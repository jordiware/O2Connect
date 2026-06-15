using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Dto.Management.Clients;

public sealed record ClientSearchRequest
{
    [FromForm(Name = "name")]
    public string? Name { get; init; }

    [FromForm(Name = "status")]
    public IReadOnlyList<string>? Status { get; init; }

    [FromForm(Name = "min_created_at")]
    public DateTimeOffset? MinCreatedAt { get; init; }

    [FromForm(Name = "max_created_at")]
    public DateTimeOffset? MaxCreatedAt { get; init; }

    [FromForm(Name = "min_last_modified_at")]
    public DateTimeOffset? MinLastModifiedAt { get; init; }

    [FromForm(Name = "max_last_modified_at")]
    public DateTimeOffset? MaxLastModifiedAt { get; init; }

    [FromForm(Name = "min_revoked_at")]
    public DateTimeOffset? MinRevokedAt { get; init; }

    [FromForm(Name = "max_revoked_at")]
    public DateTimeOffset? MaxRevokedAt { get; init; }

    [FromForm(Name = "grant_types")]
    public IReadOnlyList<string>? GrantTypes { get; init; }

    [FromForm(Name = "scopes")]
    public IReadOnlyList<string>? Scopes { get; init; }

    [FromForm(Name = "authentication_methods")]
    public IReadOnlyList<string>? AuthenticationMethods { get; init; }

    [FromForm(Name = "response_types")]
    public IReadOnlyList<string>? ResponseTypes { get; init; }
}
