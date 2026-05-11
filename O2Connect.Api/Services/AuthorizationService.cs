using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Models;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Services;

public interface IAuthorizationService
{
    Task<IActionResult> HandleAsync(AuthorizationRequest request);
}

public class AuthorizationService : IAuthorizationService
{
    IAuthorizationCodeRepository _authorizationCodeRepository;
    IClientRepository _clientRepository;

    public AuthorizationService(
        IAuthorizationCodeRepository authorizationCodeRepository,
        IClientRepository clientRepository)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _clientRepository = clientRepository;
    }

    public async Task<IActionResult> HandleAsync(AuthorizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId) ||
            string.IsNullOrWhiteSpace(request.RedirectUri) ||
            string.IsNullOrWhiteSpace(request.ResponseType))
        {
            return new BadRequestObjectResult(new { error = "invalid_request" });
        }

        var client = await _clientRepository.GetByIdAsync(request.ClientId);
        if (client == null)
        {
            return new BadRequestObjectResult(new { error = "invalid_client"});
        }

        var isValidRedirect = await _clientRepository.ValidateRedirectUriAsync(request.ClientId, request.RedirectUri);
        if (!isValidRedirect)
        {
            return new BadRequestObjectResult(new { error = "invalid_redirect" });
        }

        // Only support authorization_code for now
        if (!string.Equals(request.ResponseType, "code", StringComparison.Ordinal))
        {
            return new BadRequestObjectResult(new { error = "unsupported_response_type" });
        }

        // Mock authorization code
        var code = Guid.NewGuid().ToString("N");

        var authCode = new AuthorizationCode
        {
            Code = code,
            ClientId = request.ClientId,
            RedirectUri = request.RedirectUri,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            Scope = request.Scope,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _authorizationCodeRepository.StoreAsync(authCode);

        var uri = request.RedirectUri;
        var separator = uri.Contains("?") ? "&" : "?";

        var redirect = $"{uri}{separator}code={code}";

        if (!string.IsNullOrEmpty(request.State))
        {
            redirect += $"&state={Uri.EscapeDataString(request.State)}";
        }

        return new RedirectResult(redirect);
    }
}

