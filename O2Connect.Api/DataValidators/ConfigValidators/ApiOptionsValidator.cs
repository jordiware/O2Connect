using O2Connect.Api.Models.Options;

namespace O2Connect.Api.DataValidators.ConfigValidators;

public sealed class ApiOptionsValidator : IConfigValidator<ApiOptions>
{
    public void Validate(ApiOptions options)
    {
        if (!Uri.TryCreate(options.Domain, UriKind.Absolute, out _))
            throw new InvalidOperationException("Api:Domain must be a valid absolute URI.");
    }
}
