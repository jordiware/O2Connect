using O2Connect.Dto.Requests;

namespace O2Connect.Api.Models.RequestInputs;

public sealed class TokenRequestInput
{
    public string ClientId { get; init; } = default!;
    public string? ClientSecret { get; init; }
    public GrantType GrantType { get; init; }
    public string? Code { get; init; }
    public string? RedirectUri { get; init; }
    public string? CodeVerifier { get; init; }
    public ScopeSet? Scopes { get; init; }

    public static TokenRequestInput FromRequestDto(TokenRequest request)
    {
        var scopes = string.IsNullOrWhiteSpace(request.Scope) ? 
                     Array.Empty<string>() : 
                     request.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                  .Distinct(StringComparer.Ordinal)
                                  .ToArray();

        return new TokenRequestInput
        {
            ClientId = request.ClientId!,
            ClientSecret = request.ClientSecret,
            GrantType = GrantType.Parse(request.GrantType),
            Code = request.Code,
            RedirectUri = request.RedirectUri,
            CodeVerifier = request.CodeVerifier,
            Scopes = new ScopeSet(scopes)
        };
    }
}
