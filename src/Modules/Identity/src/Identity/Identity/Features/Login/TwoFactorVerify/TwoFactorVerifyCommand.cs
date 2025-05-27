using MediatR;

namespace Identity.Identity.Features.Login.TwoFactorVerify;

public record TwoFactorVerifyCommand : IRequest<TwoFactorVerifyResponse>
{
    public string UserId { get; init; }
    public string OtpCode { get; init; }
    public string TempToken { get; init; }
    public long? TenantId { get; init; }
    public string? TenantType { get; init; }
}

public class TwoFactorVerifyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string UserId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    
    public static TwoFactorVerifyResponse SuccessResult(
        string userId, 
        string accessToken, 
        string refreshToken, 
        int expiresIn) =>
        new()
        {
            Success = true,
            Message = "Authentication successful",
            UserId = userId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };
        
    public static TwoFactorVerifyResponse FailureResult(string message) =>
        new()
        {
            Success = false,
            Message = message
        };
} 