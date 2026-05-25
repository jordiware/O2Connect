using O2Connect.Api.Models;
using O2Connect.Api.Models.RequestInputs;
using O2Connect.Dto.Requests;

namespace O2Connect.Api.Controllers.RequestModelValidators;

public interface ITokenRequestValidator
{
    GrantType GrantType { get; }
    TokenRequestInput Validate(TokenRequest request);
}
