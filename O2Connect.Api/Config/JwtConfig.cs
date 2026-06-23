using Microsoft.Extensions.Options;
using O2Connect.Api.Config.Options;

namespace O2Connect.Api.Config;

public interface IJwtConfig
{
    string Audience { get; }
    string Issuer { get; }
    int AccessTokenLifetimeSeconds { get; }
    int RefreshTokenLifetimeDays { get; }
    SigningOptions Signing { get; }
}

public sealed class JwtConfig : IJwtConfig
{
    private readonly JwtOptions _options;

    public JwtConfig(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string Audience => _options.Audience;
    public string Issuer => _options.Issuer;
    public int AccessTokenLifetimeSeconds => _options.AccessTokenLifetimeSeconds;
    public int RefreshTokenLifetimeDays => _options.RefreshTokenLifetimeDays;
    public SigningOptions Signing => _options.Signing;
}
