using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Repositories;
using O2Connect.Api.RequestDtos;

namespace O2Connect.Api.Services;

public interface IAuthorizationService
{
    Task<IActionResult> HandleAsync(AuthorizationRequest request);
}

public class AuthorizationService : IAuthorizationService
{
    IClientRepository _clientService;

    public AuthorizationService(IClientRepository clientService)
    {
        _clientService = clientService;
    }

    public async Task<IActionResult> HandleAsync(AuthorizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientId) ||
            string.IsNullOrWhiteSpace(request.RedirectUri) ||
            string.IsNullOrWhiteSpace(request.ResponseType))
        {
            return new BadRequestObjectResult(new { error = "invalid_request" });
        }

        var client = await _clientService.GetByIdAsync(request.ClientId);
        if (client == null)
        {
            return new BadRequestObjectResult(new { error = "invalid_client"});
        }

        var isValidRedirect = await _clientService.ValidateRedirectUriAsync(request.ClientId, request.RedirectUri);
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
        var code = "mock_auth_code";

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

