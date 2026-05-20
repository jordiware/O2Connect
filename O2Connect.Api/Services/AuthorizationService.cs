using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Repositories;
using O2Connect.Dto.Requests;
using System.Security.Cryptography;

namespace O2Connect.Api.Services;

public interface IAuthorizationService
{
    Task<IActionResult> HandleAsync(AuthorizationRequest request, CancellationToken ct);
}

public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IClientRepository _clientRepository;

    public AuthorizationService(
        IAuthorizationCodeRepository authorizationCodeRepository,
        IClientRepository clientRepository)
    {
        _authorizationCodeRepository = authorizationCodeRepository;
        _clientRepository = clientRepository;
    }

    public async Task<IActionResult> HandleAsync(AuthorizationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId) ||
            string.IsNullOrWhiteSpace(request.RedirectUri) ||
            string.IsNullOrWhiteSpace(request.ResponseType))
        {
            return new BadRequestObjectResult(new { error = "invalid_request" });
        }

        var client = await _clientRepository.GetByIdAsync(request.ClientId, ct);
        if (client == null)
        {
            return BuildErrorRedirect(request.RedirectUri, "invalid_client", request.State);
        }

        var isValidRedirect = await _clientRepository.ValidateRedirectUriAsync(request.ClientId, request.RedirectUri, ct);
        if (!isValidRedirect)
        {
            return BuildErrorRedirect(request.RedirectUri, "invalid_redirect", request.State);
        }

        // Only support authorization_code for now
        if (!string.Equals(request.ResponseType, "code", StringComparison.Ordinal))
        {
            return BuildErrorRedirect(request.RedirectUri, "unsupported_response_type", request.State);
        }

        if (string.IsNullOrWhiteSpace(request.CodeChallenge))
        {
            return BuildErrorRedirect(request.RedirectUri, "invalid_request", request.State);
        }

        if (!string.Equals(request.CodeChallengeMethod, "S256", StringComparison.Ordinal))
        {
            return BuildErrorRedirect(request.RedirectUri, "invalid_request", request.State);
        }

        // Mock authorization code
        var code = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var authCode = new AuthorizationCode
        {
            Code = code,
            ClientId = request.ClientId,
            RedirectUri = request.RedirectUri,
            CodeChallenge = request.CodeChallenge,
            CodeChallengeMethod = request.CodeChallengeMethod,
            Scopes = new ScopeSet([ request.Scope! ]),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _authorizationCodeRepository.StoreAsync(authCode, ct);

        var uri = request.RedirectUri;
        var separator = uri.Contains("?") ? "&" : "?";

        var redirect = $"{uri}{separator}code={code}";

        if (!string.IsNullOrWhiteSpace(request.State))
        {
            redirect += $"&state={Uri.EscapeDataString(request.State)}";
        }

        return new RedirectResult(redirect);
    }

    private IActionResult BuildErrorRedirect(string uri, string error, string? state)
    {
        var separator = uri.Contains("?") ? "&" : "?";
        var redirect = $"{uri}{separator}error={error}";

        if (!string.IsNullOrEmpty(state))
        {
            redirect += $"&state={Uri.EscapeDataString(state)}";
        }

        return new RedirectResult(redirect);
    }
}
