using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using O2Connect.Api.Controllers.RequestModelValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using System.Text;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/token")]
public class TokenController : ControllerBase
{
    private readonly ITokenRequestValidatorResolver _requestValidatorResolver;
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ITokenService _tokenService;

    public TokenController(
        ITokenRequestValidatorResolver requestValidatorResolver,
        IClientAuthenticationService clientAuthenticationService,
        ITokenService tokenService)
    {
        _requestValidatorResolver = requestValidatorResolver;
        _clientAuthenticationService = clientAuthenticationService;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        if (!Request.HasFormContentType)
            throw OAuthException.FromInvalidRequest();

        if (Request.Headers.Authorization.Any() &&
            (!string.IsNullOrWhiteSpace(request.ClientId) || !string.IsNullOrWhiteSpace(request.ClientSecret)))
        {
            throw OAuthException.FromInvalidRequest("Client credentials must not be provided in both Authorization header and request body.");
        }

        var (clientId, clientSecret) = GetClientCredentials(request, Request.Headers.Authorization);

        request.ClientId = clientId;
        request.ClientSecret = clientSecret;

        var client = await _clientAuthenticationService.AuthenticateAsync(Request, request, HttpContext.RequestAborted);

        var grantType = GrantType.Parse(request.GrantType);
        var validator = _requestValidatorResolver.Resolve(grantType);
        var input = validator.Validate(request);

        var response = await _tokenService.HandleAsync(input, HttpContext.RequestAborted);

        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        return Ok(response);
    }

    private (string? clientId, string? clientSecret) GetClientCredentials(TokenRequest request, StringValues authorizationHeaders)
    {
        var header = authorizationHeaders.FirstOrDefault();

        if (header?.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase) == true)
        {
            var encoded = header["Basic ".Length..].Trim();

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var separatorIndex = decoded.IndexOf(':');

                if (separatorIndex <= 0)
                    throw OAuthException.FromInvalidClient();

                var clientId = decoded[..separatorIndex];
                var clientSecret = decoded[(separatorIndex + 1)..];

                return (clientId, clientSecret);
            }
            catch
            {
                throw OAuthException.FromInvalidClient();
            }
        }

        return (request.ClientId, request.ClientSecret);
    }
}
