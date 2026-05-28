using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Models;

public class ClientAuthenticationResult
{
    public Client Client { get; init; }
    public ClientAuthenticationMethod Method { get; init; }

    public ClientAuthenticationResult(
        Client client,
        ClientAuthenticationMethod method)
    {
        Client = client;
        Method = method;
    }
}