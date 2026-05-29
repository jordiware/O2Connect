using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Models.Context;

public sealed class TokenRequestContext
{
    // Core
    public required Client Client { get; init; }
    public required GrantType GrantType { get; init; }

    // _client authentication context
    public required ClientAuthenticationMethod ClientAuthenticationMethod { get; init; }
    public string ClientId => Client.ClientId;

    // Authorization Code grant
    public string? Code { get; init; }
    public string? RedirectUri { get; init; }
    public string? CodeVerifier { get; init; }

    // Refresh Token grant
    public string? RefreshToken { get; init; }

    // Device Code grant
    public string? DeviceCode { get; init; }

    // Scopes / resources
    public ValueSet Scopes { get; init; } = new([]);
    /// <summary>
    /// RFC 8707 resource indicators
    /// </summary>
    public ValueSet Resources { get; init; } = new([]);
    /// <summary>
    /// Non-standard but widely used (Auth0-style)
    /// </summary>
    public ValueSet Audiences { get; init; } = new([]);

    // Token shaping
    /// <summary>
    /// Optional requested token lifetime override (if you support it)
    /// </summary>
    public TimeSpan? RequestedAccessTokenLifetime { get; init; }

    // Proof-of-Possession / advanced security
    /// <summary>
    /// DPoP proof JWT (if present)
    /// </summary>
    public string? DPoPProof { get; init; }

    /// <summary>
    /// DPoP JWK thumbprint (computed during validation)
    /// </summary>
    public string? DPoPThumbprint { get; init; }

    /// <summary>
    /// MTLS client certificate thumbprint
    /// </summary>
    public string? ClientCertificateThumbprint { get; init; }

    // _client assertion (JWT-based auth)
    public string? ClientAssertionType { get; init; }
    public string? ClientAssertion { get; init; }

    // Actor / delegation (future-proofing)
    /// <summary>
    /// For token exchange (RFC 8693)
    /// </summary>
    public string? SubjectToken { get; init; }
    public string? SubjectTokenType { get; init; }
    public string? ActorToken { get; init; }
    public string? ActorTokenType { get; init; }

    // Additional context
    public string? RawScope { get; init; }
    public string? RequestId { get; init; }
    public DateTimeOffset RequestedAt { get; init; } = DateTimeOffset.UtcNow;
}
