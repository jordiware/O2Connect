using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.DataValidators;

public interface IScopeValidator
{
    ValueSet Validate(ValueSet? requested, Client client);
}

public class ScopeValidator : IScopeValidator
{
    public ValueSet Validate(ValueSet? requested, Client client)
    {
        var allowed = new ValueSet(client.AllowedScopes);

        if (requested == null)
            throw OAuthException.FromInvalidScope();

        if (!requested.IsSubsetOf(allowed.Values))
            throw OAuthException.FromInvalidScope();

        return requested;
    }

}
