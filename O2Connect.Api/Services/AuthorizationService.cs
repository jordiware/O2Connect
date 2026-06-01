using O2Connect.Api.Models;
using O2Connect.Dto.Requests;
using System.Security.Claims;

namespace O2Connect.Api.Services;

public interface IAuthorizationService
{
    Task<AuthorizationResult> HandleAsync(AuthorizationRequest request, ClaimsPrincipal user);
}

public class AuthorizationService : IAuthorizationService
{
    public Task<AuthorizationResult> HandleAsync(AuthorizationRequest request, ClaimsPrincipal user)
    {
        throw new NotImplementedException();
    }
}
