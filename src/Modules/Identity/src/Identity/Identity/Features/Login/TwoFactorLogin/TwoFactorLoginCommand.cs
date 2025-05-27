using MediatR;

namespace Identity.Identity.Features.Login.TwoFactorLogin;

public record TwoFactorLoginCommand : IRequest<TwoFactorLoginResponse>
{
    public string Email { get; init; }
    public string Password { get; init; }
}

public class TwoFactorLoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string UserId { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string TempToken { get; set; }
    
    public static TwoFactorLoginResponse SuccessResult(string userId, string tempToken) =>
        new()
        {
            Success = true,
            Message = "Authentication successful, OTP required",
            UserId = userId,
            RequiresTwoFactor = true,
            TempToken = tempToken
        };
        
    public static TwoFactorLoginResponse FailureResult(string message) =>
        new()
        {
            Success = false,
            Message = message,
            RequiresTwoFactor = false
        };
} 