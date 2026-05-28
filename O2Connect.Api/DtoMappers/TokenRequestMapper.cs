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

        var scopes = StringDataSet.Split(request.Scope, ' ');
        var resources = StringDataSet.Split(request.Resource, ',');
        var audiences = StringDataSet.Split(request.Audience, ',');

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
            Scopes = new ScopeSet(scopes),
            Resources = new ResourceSet(resources),
            Audiences = new AudienceSet(audiences),
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
