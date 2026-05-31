using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Controllers.RequestModelValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/token")]
public class TokenController : OidcOAuthControllerBase
{
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ITokenRequestValidatorResolver _requestValidatorResolver;
    private readonly ITokenService _tokenService;

    public TokenController(
        IClientAuthenticationService clientAuthenticationService,
        ITokenRequestValidatorResolver requestValidatorResolver,
        ITokenService tokenService)
    {
        _clientAuthenticationService = clientAuthenticationService;
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

        var grantType = GrantType.Parse(request.GrantType);

        var clientAuthenticationResult = await _clientAuthenticationService
            .AuthenticateAsync(Request, request, HttpContext.RequestAborted);

        if (!clientAuthenticationResult.Success)
            throw OAuthException.FromInvalidClient();

        var client = clientAuthenticationResult.Client;
        var method = clientAuthenticationResult.Method;

        if (!_requestValidatorResolver.TryResolve(grantType, out var requestValidator))
            throw OAuthException.FromInvalidGrant();

        var requestContext = await requestValidator
            .ValidateAsync(request, client, method, HttpContext.RequestAborted);

        var response = await _tokenService.HandleAsync(requestContext, HttpContext.RequestAborted);

        return OkJsonResponse(response);
    }
}
