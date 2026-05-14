using O2Connect.Api.Exceptions;
using O2Connect.Api.Services.TokenGrantHandlers;
using O2Connect.Api.Services.Validators;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface ITokenService
{
    Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct);
}

public class TokenService : ITokenService
{
    private readonly IClientValidator _clientValidator;
    private readonly IReadOnlyDictionary<string, ITokenGrantHandler> _grantHandlers;

    public TokenService(IClientValidator clientValidator,
        IEnumerable<ITokenGrantHandler> grantHandlers)
    {
        _clientValidator = clientValidator;
        _grantHandlers = grantHandlers.ToDictionary(h => h.GrantType, StringComparer.Ordinal);
    }

    public async Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct)
    {
        var validatedClient = await _clientValidator.ValidateAsync(request, ct);

        if (!_grantHandlers.TryGetValue(request.GrantType, out var handler))
            throw new OAuthException("unsupported_grant_type");

        return await handler.HandleAsync(request, validatedClient, ct);
    }
}
