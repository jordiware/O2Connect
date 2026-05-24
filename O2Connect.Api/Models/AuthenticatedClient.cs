using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Models;

public class AuthenticatedClient
{
    public string ClientId { get; init; } = default!;
    public Client Client { get; init; } = default!;

    public ClientAuthenticationMethod AuthenticationMethod { get; init; } = default!;
}
