using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;

namespace O2Connect.Api.Config;

public interface IOAuthConfig
{
    bool AllowPlainPkce { get; }
    bool AllowRefreshTokenReuse { get; }
    bool RequirePkce { get; }
    bool ReuseDetectionEnabled { get; }
    bool RotateRefreshTokens { get; }
}

public sealed class OAuthConfig : IOAuthConfig
{
    private readonly OAuthOptions _options;

    public OAuthConfig(IOptions<OAuthOptions> options)
    {
        _options = options.Value;
    }

    public bool RequirePkce => _options.RequirePkce;
    public bool AllowPlainPkce => _options.AllowPlainPkce;
    public bool AllowRefreshTokenReuse => _options.AllowRefreshTokenReuse;
    public bool RotateRefreshTokens => _options.RotateRefreshTokens;
    public bool ReuseDetectionEnabled => _options.ReuseDetectionEnabled;
}
