namespace O2Connect.Api.Models;

public sealed record AuthorizationRequestData
{
    public required string ResponseType { get; init; }
    public required string ClientId { get; init; }
    public required string RedirectUri { get; init; }

    public string? Scope { get; init; }
    public string? State { get; init; }
    public string? Nonce { get; init; }

    public string? CodeChallenge { get; init; }
    public string? CodeChallengeMethod { get; init; }

    public string? ResponseMode { get; init; }

    private IReadOnlySet<string>? _scopes;

    public IReadOnlySet<string> GetScopes()
    {
        if (_scopes is not null)
            return _scopes;

        _scopes = Scope?
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet()
            ?? new HashSet<string>();

        return _scopes;
    }

    public string GetResponseMode()
    {
        if (!string.IsNullOrWhiteSpace(ResponseMode))
            return ResponseMode;

        var types = ResponseType
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (types.Contains("token") || types.Contains("id_token"))
            return "fragment";

        if (types.Contains("code"))
            return "query";

        return "query";
    }
}
