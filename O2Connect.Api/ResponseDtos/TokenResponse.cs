namespace O2Connect.Api.ResponseDtos;

public class TokenResponse
{
    public string AccessToken { get; set; } = default!;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public string? IdToken { get; set; }
}

