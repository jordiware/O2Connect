using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public interface ITokenRequestValidatorResolver
{
    ITokenRequestValidator Resolve(GrantType grantType);
}

public class TokenRequestValidatorResolver : ITokenRequestValidatorResolver
{
    private readonly IReadOnlyDictionary<string, ITokenRequestValidator> _validators;

    public TokenRequestValidatorResolver(IEnumerable<ITokenRequestValidator> validators)
    {
        _validators = validators.ToDictionary(v => v.GrantType.Value, StringComparer.OrdinalIgnoreCase);
    }
    public ITokenRequestValidator Resolve(GrantType grantType)
    {
        if (!_validators.TryGetValue(grantType.Value, out var validator))
            throw OAuthException.FromInvalidGrant();

        return validator;
    }
}
