using O2Connect.Api.Config.Options;

namespace O2Connect.Api.Config.Validators;

public sealed class OAuthOptionsValidator : IConfigValidator<OAuthOptions>
{
    public void Validate(OAuthOptions options)
    {
        if (options.RequirePkce && options.AllowPlainPkce)
            throw new InvalidOperationException("OAuth: Plain PKCE cannot be allowed when PKCE is required.");

        if (!options.RotateRefreshTokens && options.ReuseDetectionEnabled)
            throw new InvalidOperationException("OAuth: Reuse detection requires refresh token rotation.");
    }
}
