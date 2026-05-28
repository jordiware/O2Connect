using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Models.Store;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.DtoMappers;

public static class TokenRequestMapper
{
    public static TokenRequestContext ToRequestContext(this TokenRequest request,
                                                        Client client,
                                                        ClientAuthenticationMethod method,
                                                        string? requestId = null,
                                                        string? dpopProof = null,
                                                        string? clientCertThumbprint = null)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return new TokenRequestContext
        {
            // Core
            Client = client,
            GrantType = GrantType.Parse(request.GrantType),
            // Client auth
            ClientAuthenticationMethod = method,
            // Authorization Code
            Code = request.Code,
            CodeVerifier = request.CodeVerifier,
            RedirectUri = request.RedirectUri,
            // Refresh Token
            RefreshToken = request.RefreshToken,
            // Device Code
            DeviceCode = request.DeviceCode,
            // Scopes / resources
            Scopes = ValueSet.FromDataString(request.Scope, ' '),
            Resources = ValueSet.FromDataString(request.Resource, ','),
            Audiences = ValueSet.FromDataString(request.Audience, ','),
            RawScope = request.Scope,
            // Client assertion
            ClientAssertion = request.ClientAssertion,
            ClientAssertionType = request.ClientAssertionType,
            // Advanced security
            DPoPProof = dpopProof,
            ClientCertificateThumbprint = clientCertThumbprint,
            // Metadata
            RequestId = requestId,
            RequestedAt = DateTimeOffset.UtcNow
        };
    }
}
