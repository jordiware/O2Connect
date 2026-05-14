using O2Connect.Api.Services.PkceValidators;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services.TokenGrantHandlers;

public interface ITokenGrantHandler
{
    public string GrantType { get; }
    Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct);
}

public abstract class TokenGrantHandler : ITokenGrantHandler
{
    protected readonly IEnumerable<IPkceValidator> _pkceValidators;

    protected TokenGrantHandler(IEnumerable<IPkceValidator> pkceValidators)
    {
        _pkceValidators = pkceValidators;
    }

    public abstract string GrantType { get; }
    public abstract Task<TokenResponse> HandleAsync(TokenRequest request, CancellationToken ct);
}
