using O2Connect.Api.Config.Options;

namespace O2Connect.Api.Config.Validators;

public sealed class JwtOptionsValidator : IConfigValidator<JwtOptions>
{
    public void Validate(JwtOptions options)
    {
        if (!Uri.TryCreate(options.Issuer, UriKind.Absolute, out _))
            throw new InvalidOperationException("Jwt:Issuer must be a valid absolute URI.");

        if (string.IsNullOrWhiteSpace(options.Audience))
            throw new InvalidOperationException("Jwt:Audience is required.");

        if (options.AccessTokenLifetimeSeconds <= 0)
            throw new InvalidOperationException("Jwt:AccessTokenLifetimeSeconds must be > 0.");

        if (options.RefreshTokenLifetimeDays <= 0)
            throw new InvalidOperationException("Jwt:RefreshTokenLifetimeDays must be > 0.");

        if (string.IsNullOrWhiteSpace(options.Signing.KeyId))
            throw new InvalidOperationException("Jwt:Signing:KeyId is required.");

        if (!File.Exists(options.Signing.PrivateKeyPath))
            throw new InvalidOperationException("Jwt:Signing:PrivateKeyPath not found.");

        if (!File.Exists(options.Signing.PublicKeyPath))
            throw new InvalidOperationException("Jwt:Signing:PublicKeyPath not found.");
    }
}
