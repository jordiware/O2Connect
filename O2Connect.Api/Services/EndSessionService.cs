using Microsoft.IdentityModel.Tokens;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace O2Connect.Api.Services;

public interface IEndSessionService
{
    Task HandleAsync(EndSessionRequest request, CancellationToken ct);
}

public class EndSessionService : IEndSessionService
{
    private static readonly JwtSecurityTokenHandler TokenHandler = new();

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public EndSessionService(
        IRefreshTokenRepository refreshTokenRepository,
        TokenValidationParameters tokenValidationParameters)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenValidationParameters = tokenValidationParameters;
    }

    public async Task HandleAsync(EndSessionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.IdTokenHint))
            return;

        ClaimsPrincipal principal;

        try
        {
            principal = TokenHandler.ValidateToken(request.IdTokenHint,
                                                    _tokenValidationParameters,
                                                    out var validatedToken);

            if (validatedToken is not JwtSecurityToken)
                return;
        }
        catch (SecurityTokenException)
        {
            return;
        }

        var sessionId = principal.FindFirst("sid")?.Value;

        if (!string.IsNullOrWhiteSpace(sessionId))
            await _refreshTokenRepository.RevokeSessionAsync(sessionId, ct);
    }
}
