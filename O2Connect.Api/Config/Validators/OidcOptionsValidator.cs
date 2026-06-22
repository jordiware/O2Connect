using O2Connect.Api.Config.OptionsModels;

namespace O2Connect.Api.Config.Validators;

public sealed class OidcOptionsValidator : IConfigValidator<OidcOptions>
{
    private static readonly HashSet<string> RequiredScopes =
    [
        "openid"
    ];

    private static readonly HashSet<string> RequiredResponseTypes =
    [
        "code"
    ];

    public void Validate(OidcOptions options)
    {
        if (!Uri.TryCreate(options.Issuer, UriKind.Absolute, out _))
            throw new InvalidOperationException("Oidc:Issuer must be a valid absolute URI.");

        ValidatePath(options.AuthorizationEndpoint, "AuthorizationEndpoint");
        ValidatePath(options.TokenEndpoint, "TokenEndpoint");
        ValidatePath(options.UserInfoEndpoint, "UserInfoEndpoint");
        ValidatePath(options.JwksEndpoint, "JwksEndpoint");
        ValidatePath(options.EndSessionEndpoint, "EndSessionEndpoint");

        if (options.ScopesSupported is null || options.ScopesSupported.Count == 0)
            throw new InvalidOperationException("Oidc:ScopesSupported must contain at least one scope.");

        foreach (var required in RequiredScopes)
        {
            if (!options.ScopesSupported.Contains(required))
                throw new InvalidOperationException($"Oidc:ScopesSupported must include '{required}'.");
        }

        if (options.ResponseTypesSupported is null || options.ResponseTypesSupported.Count == 0)
            throw new InvalidOperationException("Oidc:ResponseTypesSupported must contain at least one value.");

        foreach (var required in RequiredResponseTypes)
        {
            if (!options.ResponseTypesSupported.Contains(required))
                throw new InvalidOperationException($"Oidc:ResponseTypesSupported must include '{required}'.");
        }
    }

    private static void ValidatePath(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Oidc:{name} is required.");

        if (!value.StartsWith('/'))
            throw new InvalidOperationException($"Oidc:{name} must be a relative path starting with '/'.");
    }
}
