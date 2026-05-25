namespace O2Connect.Api.Models;

public class ClientAuthenticationResult
{
    public bool Succeeded { get; }
    public string? ClientId { get; }

    private ClientAuthenticationResult(bool succeeded, string? clientId)
    {
        Succeeded = succeeded;
        ClientId = clientId;
    }

    public static ClientAuthenticationResult Success(string clientId)
        => new(true, clientId);

    public static ClientAuthenticationResult Fail()
        => new(false, null);
}