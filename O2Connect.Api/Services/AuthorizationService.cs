using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.RequestDtos;

namespace O2Connect.Api.Services;

public interface IAuthorizationService
{
    Task<IActionResult> HandleAsync(AuthorizationRequest request);
}

public class AuthorizationService : IAuthorizationService
{
    public Task<IActionResult> HandleAsync(AuthorizationRequest request)
    {
        // Mock validation
        if (string.IsNullOrWhiteSpace(request.ClientId) ||
            string.IsNullOrWhiteSpace(request.RedirectUri) ||
            string.IsNullOrWhiteSpace(request.ResponseType))
        {
            return Task.FromResult<IActionResult>(new BadRequestObjectResult(new
            {
                error = "invalid_request"
            }));
        }

        // Only support authorization_code for now
        if (!string.Equals(request.ResponseType, "code", StringComparison.Ordinal))
        {
            return Task.FromResult<IActionResult>(new BadRequestObjectResult(new
            {
                error = "unsupported_response_type"
            }));
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

        return Task.FromResult<IActionResult>(new RedirectResult(redirect));
    }
}

