using O2Connect.Api.Models;
using O2Connect.Api.Services.Validators;
using O2Connect.Dto.Requests;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services.TokenGrantHandlers;

public interface ITokenGrantHandler
{
    public string GrantType { get; }
    Task<TokenResponse> HandleAsync(TokenRequest request, ValidatedClient client, CancellationToken ct);
}

public abstract class TokenGrantHandler : ITokenGrantHandler
{
    protected readonly IReadOnlyDictionary<string, IPkceValidator> _pkceValidators;

    protected TokenGrantHandler(IEnumerable<IPkceValidator> pkceValidators)
    {
        _pkceValidators = pkceValidators.ToDictionary(v => v.Method, StringComparer.Ordinal);
    }

    public abstract string GrantType { get; }
    public abstract Task<TokenResponse> HandleAsync(TokenRequest request, ValidatedClient client, CancellationToken ct);
}
