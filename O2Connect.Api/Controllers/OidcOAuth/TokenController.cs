using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using O2Connect.Api.Controllers.RequestModelValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Models.Context;
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
        if (!Request.ContentType?.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) == true)
            throw OAuthException.FromInvalidRequest();

        if (Request.Form["client_id"].Count > 1)
            throw OAuthException.FromInvalidRequest("client_id repeated");

        if (Request.Headers.Authorization.Any()
            && (!string.IsNullOrWhiteSpace(request.ClientId) || !string.IsNullOrWhiteSpace(request.ClientSecret)))
        {
            throw OAuthException.FromInvalidRequest("Client credentials must not be provided in both Authorization header and request body.");
        }

        var authenticatedClient = await _clientAuthenticationService
            .AuthenticateAsync(Request, request, HttpContext.RequestAborted);

        if (!authenticatedClient.Succeeded)
            throw OAuthException.FromInvalidClient();

        var grantType = GrantType.Parse(request.GrantType);
        var validator = _requestValidatorResolver.Resolve(grantType);
        var tokenRequestContext = validator.Validate(request, authenticatedClient.Client!, authenticatedClient.Method!.Value);

        var response = await _tokenService.HandleAsync(tokenRequestContext, HttpContext.RequestAborted);

        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        return Ok(response);
    }
}
