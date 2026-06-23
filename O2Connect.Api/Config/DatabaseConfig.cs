using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;

namespace O2Connect.Api.Config;

public interface IDatabaseConfig
{
    string ConnectionString { get; }
    string Provider { get; }
}

public sealed class DatabaseConfig : IDatabaseConfig
{
    private readonly DatabaseOptions _options;

    public DatabaseConfig(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public string Provider => _options.Provider;
    public string ConnectionString => _options.ConnectionString;
}
