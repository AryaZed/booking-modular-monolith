using Identity.Identity.Models;
using MediatR;

namespace Identity.Identity.Features.Otp.VerifyOtp;

public record VerifyOtpCommand : IRequest<VerifyOtpResponse>
{
    public string UserId { get; init; }
    public string Code { get; init; }
    public OtpPurpose Purpose { get; init; }
}

public class VerifyOtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string UserId { get; set; }
    public OtpPurpose Purpose { get; set; }
    
    public static VerifyOtpResponse SuccessResult(string userId, OtpPurpose purpose) =>
        new()
        {
            Success = true,
            Message = "OTP verified successfully",
            UserId = userId,
            Purpose = purpose
        };
        
    public static VerifyOtpResponse FailureResult(string message) =>
        new()
        {
            Success = false,
            Message = message
        };
} 