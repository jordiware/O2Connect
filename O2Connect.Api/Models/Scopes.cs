namespace O2Connect.Api.Models;

public static class Scopes
{
    public static IReadOnlyList<string> All { get; } = new List<string>
    {
        Oidc.OpenId,
        Oidc.Profile,
        Oidc.Email,
        Oidc.Address,
        Oidc.Phone,
        OAuth.OfflineAccess,
        Users.Read,
        Users.Query,
        Users.Write,
        Users.Delete,
        Clients.Read,
        Clients.Query,
        Clients.Write,
        Clients.Delete,
        Account.Login,
        Account.Register,
        Account.Delete,
        Profile.Read,
        Profile.Write,
        Sessions.Read,
        Sessions.Revoke,
        Tokens.Read,
        Tokens.Revoke,
        Tokens.Introspect,
        Api.Read,
        Api.Write,
        Security.JwksRead
    };

    /// <summary>
    /// OpenID Connect standard identity scopes.
    /// </summary>
    public static class Oidc
    {
        public const string OpenId = "openid";
        public const string Profile = "profile";
        public const string Email = "email";
        public const string Address = "address";
        public const string Phone = "phone";
    }

    /// <summary>
    /// OAuth2 standard/common scopes.
    /// </summary>
    public static class OAuth
    {
        public const string OfflineAccess = "offline_access";
    }

    /// <summary>
    /// User identity and profile management scopes.
    /// </summary>
    public static class Users
    {
        public const string Query = "users.query";
        public const string Read = "users.read";
        public const string Write = "users.write";
        public const string Delete = "users.delete";
    }

    /// <summary>
    /// User own identity and account management scopes.
    /// </summary>
    public static class Account
    {
        public const string Login = "account.login";
        public const string Register = "account.register";
        public const string Delete = "account.delete";
    }

    /// <summary>
    /// Self-service profile scopes (end-user context).
    /// </summary>
    public static class Profile
    {
        public const string Read = "profile.read";
        public const string Write = "profile.write";
    }

    /// <summary>
    /// Session and authentication session management scopes.
    /// </summary>
    public static class Sessions
    {
        public const string Read = "sessions.read";
        public const string Revoke = "sessions.revoke";
    }

    /// <summary>
    /// OAuth client management (dynamic registration, client lifecycle).
    /// </summary>
    public static class Clients
    {
        public const string Query = "clients.query";
        public const string Read = "clients.read";
        public const string Write = "clients.write";
        public const string Delete = "clients.delete";
    }

    /// <summary>
    /// Token lifecycle and security operations.
    /// </summary>
    public static class Tokens
    {
        public const string Read = "tokens.read";
        public const string Revoke = "tokens.revoke";
        public const string Introspect = "token.introspect";
    }

    /// <summary>
    /// System-level API access scopes (generic service access).
    /// </summary>
    public static class Api
    {
        public const string Read = "api.read";
        public const string Write = "api.write";
    }

    /// <summary>
    /// Security infrastructure scopes (JWKS, discovery, etc.).
    /// </summary>
    public static class Security
    {
        public const string JwksRead = "jwks.read";
    }
}
