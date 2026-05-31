using O2Connect.Api.DataFactories;
using O2Connect.Api.DataFactories.RequestModels;
using O2Connect.Api.DataValidators.Crypto;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.DataHandlers.TokenGrantHandlers;

public class AuthorizationCodeGrantHandler : ITokenGrantHandler
{
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly ITokenFactory _tokenFactory;

    public GrantType GrantType => GrantType.AuthorizationCode;

    public AuthorizationCodeGrantHandler(
        IAuthorizationCodeRepository authorizationCodeRepository,
        ITokenFactory tokenFactory)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _tokenFactory = tokenFactory;
    }

    public async Task<TokenResponse> HandleAsync(TokenRequestContext context, CancellationToken ct)
    {
        var code = await _authorizationCodeRepository.RedeemAsync(context.AuthorizationCode.Code, ct)
            ?? throw OAuthException.FromInvalidGrant();

        return await _tokenFactory.GenerateAsync(new JwtTokenFactoryRequest
        {
            Client = context.Client,
            Scopes = context.AuthorizationCode.Scopes.Values,
            Subject = code.SubjectId
        }, ct);
    }
}
