namespace Identity.Identity.Features.Login;

public record LoginResponse
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }
    public long UserId { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public long? CurrentTenantId { get; init; }
    public string CurrentTenantName { get; init; }
    public string CurrentRole { get; init; }
    public IEnumerable<string> Permissions { get; init; }
    public int ExpiresIn { get; init; }
} 