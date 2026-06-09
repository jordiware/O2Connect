using Microsoft.AspNetCore.Mvc;
using O2Connect.Api.DataValidators.TokenRequestValidators;
using O2Connect.Api.Exceptions;
using O2Connect.Api.Models;
using O2Connect.Api.Services;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.OidcOAuth;

[ApiController]
[Route("connect/token")]
public class ConnectTokenController : ControllerBase
{
    private readonly IClientAuthenticationService _clientAuthenticationService;
    private readonly ITokenRequestValidatorResolver _requestValidatorResolver;
    private readonly ITokenService _tokenService;

    public ConnectTokenController(
        IClientAuthenticationService clientAuthenticationService,
        ITokenRequestValidatorResolver requestValidatorResolver,
        ITokenService tokenService)
    {
        _clientAuthenticationService = clientAuthenticationService;
        _requestValidatorResolver = requestValidatorResolver;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Token([FromForm] TokenRequest request,
                                           CancellationToken ct)
    {
        if (!Request.HasFormContentType)
            throw OAuthException.FromInvalidRequest();
        if (!ModelState.IsValid)
            throw OAuthException.FromInvalidRequest();

        if (!GrantType.TryParse(request.GrantType, out var grantType))
            throw OAuthException.FromUnsupportedGrantType();

        var clientAuthenticationResult = await _clientAuthenticationService.AuthenticateAsync(Request,
                                                                                              request,
                                                                                              ct);

        if (!clientAuthenticationResult.Success)
            throw OAuthException.FromInvalidClient();

        var client = clientAuthenticationResult.Client;
        var method = clientAuthenticationResult.Method;

        if (!_requestValidatorResolver.TryResolve(grantType, out var requestValidator))
            throw OAuthException.FromUnsupportedGrantType();

        var requestContext = await requestValidator.ValidateAsync(request, client, method, ct);

        var response = await _tokenService.HandleAsync(requestContext, ct);

        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        return new JsonResult(response)
        {
            StatusCode = StatusCodes.Status200OK,
            ContentType = "application/json"
        };
    }
}
