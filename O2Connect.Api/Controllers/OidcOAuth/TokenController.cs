using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.Controllers.RequestModelValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;
using System.Net;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/token")]
public class TokenController : ControllerBase
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
        {
            throw OAuthException.FromInvalidRequest();
        }

        if (!ModelState.IsValid)
        {
            throw OAuthException.FromInvalidRequest();
        }

        var (result, authenticatedClient) = await _clientAuthenticationService
            .AuthenticateAsync(Request, request, HttpContext.RequestAborted);

        if (!result || authenticatedClient is null)
        {
            throw OAuthException.FromInvalidClient();
        }

        var grantType = GrantType.Parse(request.GrantType);
        var requestValidator = _requestValidatorResolver.Resolve(grantType);
        var requestContext = await requestValidator.ValidateAsync(request,
                                                                  authenticatedClient.Client,
                                                                  authenticatedClient.Method,
                                                                  HttpContext.RequestAborted);

        var response = await _tokenService.HandleAsync(requestContext, HttpContext.RequestAborted);

        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        return new JsonResult(response)
        {
            StatusCode = (int)HttpStatusCode.OK,
            ContentType = "application/json"
        };
    }
}
