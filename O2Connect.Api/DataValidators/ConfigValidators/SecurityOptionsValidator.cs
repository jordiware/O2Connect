using O2Connect.Api.Models.Options;

namespace O2Connect.Api.DataValidators.ConfigValidators;

public sealed class SecurityOptionsValidator : IConfigValidator<SecurityOptions>
{
    public void Validate(SecurityOptions options)
    {
        ValidatePassword(options.Password);
        ValidateLockout(options.Lockout);
    }

    private static void ValidatePassword(PasswordOptions options)
    {
        if (options.MinLength < 6)
            throw new InvalidOperationException("Security:Password:MinLength must be >= 6.");
    }

    private static void ValidateLockout(LockoutOptions options)
    {
        if (options.MaxFailedAttempts <= 0)
            throw new InvalidOperationException("Security:Lockout:MaxFailedAttempts must be > 0.");

        if (options.DurationMinutes <= 0)
            throw new InvalidOperationException("Security:Lockout:DurationMinutes must be > 0.");
    }
}
