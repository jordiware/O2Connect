using O2Connect.Api.Config.Options;

namespace O2Connect.Api.Config.Validators;

public sealed class DatabaseOptionsValidator : IConfigValidator<DatabaseOptions>
{
    public void Validate(DatabaseOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Provider))
            throw new InvalidOperationException("Database:Provider is required.");

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new InvalidOperationException("Database:ConnectionString is required.");

        if (!string.Equals(options.Provider, "Postgres", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Database:Provider currently only supports 'Postgres'.");
    }
}
