using O2Connect.Api.Models.SmartEnums;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Models;

public readonly record struct ClientAuthenticationResult
{
    public bool Success { get; }
    private readonly Client _client;
    private readonly ClientAuthenticationMethod _method;

    public Client Client 
        => Success ? _client : throw new InvalidOperationException("ClientAuthenticationResult does not contain a Client when authentication failed.");
    public ClientAuthenticationMethod Method 
        => Success ? _method : throw new InvalidOperationException("ClientAuthenticationResult does not contain a Method when authentication failed.");

    private ClientAuthenticationResult(
        bool success,
        Client client,
        ClientAuthenticationMethod method)
    {
        Success = success;
        _client = client;
        _method = method;
    }

    public static ClientAuthenticationResult Fail()
    {
        return new(false, default!, default);
    }

    public static ClientAuthenticationResult Ok(Client client, ClientAuthenticationMethod method)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        return new(true, client, method);
    }

    public override string ToString()
    {
        return Success ? $"Success: {Client} ({Method})" : "Failure";
    }
}
