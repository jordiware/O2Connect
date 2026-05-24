using O2Connect.Api.Crypto;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.DataValidators;

public interface IClientSecretValidator
{
    bool Validate(Client client, string? providedSecret);
}

public class ClientSecretValidator : IClientSecretValidator
{
    private readonly ISecretHasher _hasher;

    public ClientSecretValidator(ISecretHasher hasher)
    {
        _hasher = hasher;
    }

    public bool Validate(Client client, string? providedSecret)
    {
        if (!client.RequiresSecret)
            return true;

        if (string.IsNullOrEmpty(client.ClientSecret))
            return false;

        if (string.IsNullOrEmpty(providedSecret))
            return false;

        return _hasher.Verify(providedSecret, client.ClientSecret);
    }
}
