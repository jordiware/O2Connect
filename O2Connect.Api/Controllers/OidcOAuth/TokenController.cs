using Microsoft.AspNetCore.Mvc;
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
    private readonly ITokenService _tokenService;

    public TokenController(
        ITokenRequestValidatorResolver requestValidatorResolver,
        ITokenService tokenService)
    {
        _requestValidatorResolver = requestValidatorResolver;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        if (!Request.HasFormContentType)
            throw OAuthException.FromInvalidRequest();
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        var (clientId, clientSecret) = GetClientCredentials(request);

        request.ClientId = clientId;
        request.ClientSecret = clientSecret;

        var grantType = GrantType.Parse(request.GrantType);
        var validator = _requestValidatorResolver.Resolve(grantType);
        var input = validator.Validate(request);

        var response = await _tokenService.HandleAsync(input, HttpContext.RequestAborted);

        return Ok(response);
    }

    private (string? clientId, string? clientSecret) GetClientCredentials(TokenRequest request)
    {
        if (Request.Headers.Authorization.FirstOrDefault()?.StartsWith("Basic ") == true)
        {
            var encoded = Request.Headers.Authorization.ToString()["Basic ".Length..];
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var parts = decoded.Split(':', 2);

            return (parts[0], parts.Length > 1 ? parts[1] : null);
        }

        return (request.ClientId, request.ClientSecret);
    }
}
