using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Models;

public class ClientAuthenticationResult
{
    public bool Succeeded { get; }
    public Client? Client { get; init; }
    public ClientAuthenticationMethod? Method { get; init; }

    private ClientAuthenticationResult(
        bool succeeded,
        Client? client = null,
        ClientAuthenticationMethod? method = null)
    {
        Succeeded = succeeded;
        Client = client;
        Method = method;
    }

    public static ClientAuthenticationResult Success(Client client, ClientAuthenticationMethod method)
        => new(true, client, method);

    public static ClientAuthenticationResult Fail()
        => new(false);
}