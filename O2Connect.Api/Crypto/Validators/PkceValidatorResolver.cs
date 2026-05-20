using O2Connect.Api.Exceptions;

namespace O2Connect.Api.Crypto.Validators;

public interface IPkceValidatorResolver
{
    IPkceValidator Resolve(PkceMethod method);
}

public class PkceValidatorResolver : IPkceValidatorResolver
{
    private readonly IReadOnlyDictionary<string, IPkceValidator> _validators;

    public PkceValidatorResolver(IEnumerable<IPkceValidator> validators)
    {
        _validators = validators.ToDictionary(v => v.Method.Value, StringComparer.Ordinal);
    }

    public IPkceValidator Resolve(PkceMethod method)
    {
        if (!_validators.TryGetValue(method.Value, out var validator))
            throw OAuthException.FromInvalidGrant();

        return validator;
    }
}
