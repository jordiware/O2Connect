using O2Connect.Api.Models.SmartEnums;

namespace O2Connect.Api.Models.Mappers;

public static class UserRoleScopeMapper
{
    public static string ToScopeString(this UserRole role)
    {
        var scopes = role.GetScopes();

        return string.Join(' ', scopes);
    }

    public static IReadOnlyCollection<string> GetScopes(this UserRole role)
    {
        return role switch
        {
            var r when r == UserRole.Admin => Admin(),
            var r when r == UserRole.User => User(),
            var r when r == UserRole.Service => Service(),
            var r when r == UserRole.Developer => Developer(),
            var r when r == UserRole.Support => Support(),
            var r when r == UserRole.Auditor => Auditor(),
            var r when r == UserRole.Manager => Manager(),
            var r when r == UserRole.Operator => Operator(),
            _ => []
        };
    }

    private static string[] Admin() =>
    [
        Scopes.Oidc.OpenId,
        Scopes.Oidc.Profile,
        Scopes.Oidc.Email,

        Scopes.Users.Read,
        Scopes.Users.Write,
        Scopes.Users.Delete,

        Scopes.Profile.Read,
        Scopes.Profile.Write,

        Scopes.Sessions.Read,
        Scopes.Sessions.Revoke,

        Scopes.Clients.Read,
        Scopes.Clients.Write,
        Scopes.Clients.Delete,

        Scopes.Tokens.Read,
        Scopes.Tokens.Revoke,
        Scopes.Tokens.Introspect,

        Scopes.Api.Read,
        Scopes.Api.Write,

        Scopes.Security.JwksRead
    ];

    private static string[] User() =>
    [
        Scopes.Oidc.OpenId,
        Scopes.Oidc.Profile,
        Scopes.Oidc.Email,

        Scopes.Profile.Read,
        Scopes.Profile.Write,

        Scopes.Sessions.Read
    ];

    private static string[] Service() =>
    [
        Scopes.Api.Read,
        Scopes.Api.Write,
        Scopes.Tokens.Introspect,
        Scopes.Security.JwksRead
    ];

    private static string[] Developer() =>
    [
        Scopes.Clients.Read,
        Scopes.Clients.Write,
        Scopes.Security.JwksRead
    ];

    private static string[] Support() =>
    [
        Scopes.Users.Read,
        Scopes.Sessions.Read,
        Scopes.Tokens.Revoke
    ];

    private static string[] Auditor() =>
    [
        Scopes.Users.Read,
        Scopes.Clients.Read,
        Scopes.Sessions.Read,
        Scopes.Tokens.Read
    ];

    private static string[] Manager() =>
    [
        Scopes.Users.Read,
        Scopes.Users.Write,
        Scopes.Profile.Read,
        Scopes.Sessions.Read
    ];

    private static string[] Operator() =>
    [
        Scopes.Api.Read,
        Scopes.Sessions.Read
    ];
}
