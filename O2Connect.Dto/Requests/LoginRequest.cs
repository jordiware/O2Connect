namespace O2Connect.Dto.Requests;

public sealed record LoginRequest
{
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
    public bool RememberMe { get; init; } = false;
    public string? ReturnUrl { get; init; }
}
