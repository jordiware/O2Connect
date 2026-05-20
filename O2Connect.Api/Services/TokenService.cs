using O2Connect.Api.DataHandlers.TokenGrantHandlers;
using O2Connect.Api.DataValidators;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface ITokenService
{
    Task<TokenResponse> HandleAsync(TokenInput input, CancellationToken ct);
}

public class TokenService : ITokenService
{
    private readonly ITokenRequestValidator _tokenValidator;
    private readonly ITokenGrantHandlerResolver _grantResolver;

    public TokenService(
        ITokenRequestValidator tokenValidator,
        ITokenGrantHandlerResolver grantResolver)
    {
        _tokenValidator = tokenValidator;
        _grantResolver = grantResolver;
    }

    public async Task<TokenResponse> HandleAsync(TokenInput input, CancellationToken ct)
    {
        var context = await _tokenValidator.ValidateAsync(input, ct);

        var handler = _grantResolver.Resolve(context.GrantType);

        return await handler.HandleAsync(context, ct);
    }
}
