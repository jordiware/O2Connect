using O2Connect.Api.Config.Options;

namespace O2Connect.Api.Config.Validators;

public sealed class CoreConsistencyValidator
{
    public void Validate(ApiOptions api, JwtOptions jwt, OidcOptions oidc)
    {
        if (!string.Equals(api.Domain, jwt.Issuer, StringComparison.Ordinal))
            throw new InvalidOperationException("Api:Domain and Jwt:Issuer must match.");

        if (!string.Equals(api.Domain, oidc.Issuer, StringComparison.Ordinal))
            throw new InvalidOperationException("Api:Domain and Oidc:Issuer must match.");
    }
}
