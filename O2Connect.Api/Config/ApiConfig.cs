using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;

namespace O2Connect.Api.Config;

public interface IApiConfig
{
    string Domain { get; }
}

public sealed class ApiConfig : IApiConfig
{
    private readonly ApiOptions _options;

    public ApiConfig(IOptions<ApiOptions> options)
    {
        _options = options.Value;
    }

    public string Domain => _options.Domain;
}
