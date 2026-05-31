using O2Connect.Api.Models;
using System.Diagnostics.CodeAnalysis;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public interface ITokenRequestValidatorResolver
{
    bool TryResolve(GrantType grantType, [NotNullWhen(true)] out ITokenRequestValidator validator);
}

public class TokenRequestValidatorResolver : ITokenRequestValidatorResolver
{
    private readonly IReadOnlyDictionary<GrantType, ITokenRequestValidator> _validators;

    public TokenRequestValidatorResolver(IEnumerable<ITokenRequestValidator> validators)
    {
        var dict = new Dictionary<GrantType, ITokenRequestValidator>();

        foreach (var validator in validators)
        {
            if (!dict.TryAdd(validator.GrantType, validator))
                throw new InvalidOperationException($"Duplicate grant type validator registered: {validator.GrantType}");
        }

        _validators = dict;
    }

    public bool TryResolve(GrantType grantType, [NotNullWhen(true)] out ITokenRequestValidator validator)
    {
        return _validators.TryGetValue(grantType, out validator!);
    }
}
