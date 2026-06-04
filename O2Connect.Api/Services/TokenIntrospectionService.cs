using O2Connect.Api.DataValidators;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Responses;

namespace O2Connect.Api.Services;

public interface ITokenIntrospectionService
{
    Task<IntrospectionResponse> IntrospectAsync(string token,
                                                string callingClientId,
                                                CancellationToken ct);
}

public class TokenIntrospectionService : ITokenIntrospectionService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtValidator _jwtValidator;

    public TokenIntrospectionService(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtValidator jwtValidator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtValidator = jwtValidator;
    }

    public async Task<IntrospectionResponse> IntrospectAsync(string token,
                                                             string callingClientId,
                                                             CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var jwt = _jwtValidator.Validate(token);

        if (jwt.IsValid)
        {
            if (jwt.TokenType != "access_token")
                return IntrospectionResponse.Inactive;

            if (!string.IsNullOrWhiteSpace(jwt.SessionId))
            {
                var sessionValid = await _refreshTokenRepository.IsSessionActiveAsync(jwt.SessionId, ct);

                if (!sessionValid)
                    return IntrospectionResponse.Inactive;
            }

            return new IntrospectionResponse
            {
                Sub = jwt.Subject,
                ClientId = jwt.ClientId,
                Scopes = jwt.Scopes,
                Exp = jwt.ExpUnix,
                Iat = jwt.IatUnix,
                TokenType = "access_token"
            };
        }

        var refresh = await _refreshTokenRepository.GetAsync(token, ct);

        if (refresh is null || refresh.Revoked)
            return IntrospectionResponse.Inactive;

        if (refresh.ClientId != callingClientId)
            return IntrospectionResponse.Inactive;

        return new IntrospectionResponse
        {
            Sub = refresh.Subject,
            ClientId = refresh.ClientId,
            Scopes = refresh.Scopes,
            Exp = refresh.ExpiresAt.ToUnixTimeSeconds(),
            Iat = refresh.CreatedAt.ToUnixTimeSeconds(),
            TokenType = "refresh_token"
        };
    }
}
