using O2Connect.Api.Models.RequestInputs;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Models.DataContexts;

public sealed record TokenRequestContext(
    Client Client,
    GrantType GrantType,
    ScopeSet RequestedScopes,
    TokenRequestInput Input
);
