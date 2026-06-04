namespace O2Connect.Dto.Responses;

public sealed record IntrospectionResponse
{
    public static IntrospectionResponse Inactive { get; } = new()
    {
        Active = false
    };

    public bool Active { get; init; } = true;
    public string? Sub { get; init; }
    public string? ClientId { get; init; }
    public IEnumerable<string> Scopes { get; init; } = [];
    public long? Exp { get; init; }
    public long? Iat { get; init; }
    public string? TokenType { get; init; } = "access_token";
}
