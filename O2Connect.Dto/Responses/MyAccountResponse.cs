namespace O2Connect.Dto.Responses;

public sealed record MyAccountResponse
{
    public bool IsAuthenticated { get; init; }
    public string Id { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string[] Roles { get; init; } = [];
}
