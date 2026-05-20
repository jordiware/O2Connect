using O2Connect.Api.Models.RequestInputs;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Services.TokenGrantHandlers;

namespace O2Connect.Api.Models.RequestContexts;

public sealed record TokenRequestContext(
    Client Client,
    GrantType GrantType,
    ScopeSet RequestedScopes,
    TokenInput Input
);
