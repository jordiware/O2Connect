using O2Connect.Api.DataValidators;
using O2Connect.Api.Models;
using O2Connect.Api.Repositories;
using O2Connect.Dto.OidcOAuth.Connect;

namespace O2Connect.Api.Services.OidcOAuth;

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

        if (!jwt.IsValid)
            return IntrospectionResponse.Inactive;

        return jwt.TokenType switch
        {
            "access_token" => await FromAccessToken(jwt, callingClientId, ct),
            "refresh_token" => await FromRefreshToken(token, callingClientId, ct),
            _ => IntrospectionResponse.Inactive
        };
    }

    private async Task<IntrospectionResponse> FromAccessToken(JwtValidationResult jwt,
                                                              string callingClientId,
                                                              CancellationToken ct)
    {
        if (!IsValid(jwt.Subject, jwt.ClientId, callingClientId))
            return IntrospectionResponse.Inactive;

        if (!string.IsNullOrWhiteSpace(jwt.SessionId))
        {
            var sessionValid = await _refreshTokenRepository.IsSessionActiveAsync(jwt.SessionId, ct);

            if (!sessionValid)
                return IntrospectionResponse.Inactive;
        }

        return new IntrospectionResponse
        {
            Active = true,
            Sub = jwt.Subject,
            ClientId = jwt.ClientId,
            Scope = string.Join(' ', jwt.Scopes.Order()),
            Exp = jwt.ExpUnix,
            Iat = jwt.IatUnix,
            TokenType = "access_token"
        };
    }

    private async Task<IntrospectionResponse> FromRefreshToken(string token,
                                                               string callingClientId,
                                                               CancellationToken ct)
    {
        var refresh = await _refreshTokenRepository.GetAsync(token, ct);

        if (refresh is null || refresh.Revoked)
            return IntrospectionResponse.Inactive;

        if (!IsValid(refresh.Subject, refresh.ClientId, callingClientId)) 
            return IntrospectionResponse.Inactive;

        return new IntrospectionResponse
        {
            Active = true,
            Sub = refresh.Subject,
            ClientId = refresh.ClientId,
            Scope = string.Join(' ', refresh.Scopes.Order()),
            Exp = refresh.ExpiresAt.ToUnixTimeSeconds(),
            Iat = refresh.CreatedAt.ToUnixTimeSeconds(),
            TokenType = "refresh_token"
        };
    }

    private bool IsValid(string? subject, string? clientId, string? callingClientId)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return false;

        if (string.IsNullOrWhiteSpace(clientId))
            return false;

        if (string.IsNullOrWhiteSpace(callingClientId))
            return false;

        if (!clientId.Equals(callingClientId, StringComparison.Ordinal))
            return false;

        return true;
    }
}
