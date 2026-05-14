using O2Connect.Api.Exceptions;
using O2Connect.Api.Repositories;
using O2Connect.Api.Services.Helpers.TokenGrantHandlers;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface ITokenService
{
    Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct);
}

public class TokenService : ITokenService
{
    private readonly IClientRepository _clientRepository;
    private readonly IEnumerable<ITokenGrantHandler> _grantTypeHandlers;

    public TokenService(IClientRepository clientRepository,
        IEnumerable<ITokenGrantHandler> grantTypeHandlers)
    {
        _clientRepository = clientRepository;
        _grantTypeHandlers = grantTypeHandlers;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId))
            throw new OAuthException("invalid_request", "client_id is empty");

        if (string.IsNullOrWhiteSpace(request.GrantType))
            throw new OAuthException("invalid_request", "grant_type is empty");

        if (string.IsNullOrWhiteSpace(request.Scope))
            throw new OAuthException("invalid_request", "scope is empty");

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);

        if (client == null)
            throw new OAuthException("invalid_client", "No client found for given client_id");

        var requestedScopes = request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (!requestedScopes.All(client.AllowedScopes.Contains))
            throw new OAuthException("invalid_scope");

        if (client.RequiresSecret
            && !await _clientRepository.ValidateClientAsync(request.ClientId, request.ClientSecret, ct))
        {
            throw new OAuthException("invalid_client");
        }

        var grantHandler = _grantTypeHandlers.FirstOrDefault(h => h.GrantType == request.GrantType);

        if (grantHandler == null)
            throw new OAuthException("unsupported_grant_type");

        return await grantHandler.HandleAsync(request, ct);
    }
}
