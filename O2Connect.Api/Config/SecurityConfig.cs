using Microsoft.Extensions.Options;
using O2Connect.Api.Config.OptionsModels;

namespace O2Connect.Api.Config;

public interface ISecurityConfig
{
    LockoutOptions Lockout { get; }
    PasswordOptions Password { get; }
}

public sealed class SecurityConfig : ISecurityConfig
{
    private readonly SecurityOptions _options;

    public SecurityConfig(IOptions<SecurityOptions> options)
    {
        _options = options.Value;
    }

    public PasswordOptions Password => _options.Password;
    public LockoutOptions Lockout => _options.Lockout;
}
