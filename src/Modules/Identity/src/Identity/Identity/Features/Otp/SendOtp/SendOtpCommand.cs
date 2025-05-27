using Identity.Identity.Models;
using MediatR;

namespace Identity.Identity.Features.Otp.SendOtp;

public record SendOtpCommand : IRequest<SendOtpResponse>
{
    public string UserId { get; init; }
    public OtpPurpose Purpose { get; init; }
    public string? Reference { get; init; }
}

public class SendOtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string UserId { get; set; }
    public OtpPurpose Purpose { get; set; }
    
    public static SendOtpResponse SuccessResult(string userId, OtpPurpose purpose) =>
        new()
        {
            Success = true,
            Message = "OTP sent successfully",
            UserId = userId,
            Purpose = purpose
        };
        
    public static SendOtpResponse FailureResult(string message) =>
        new()
        {
            Success = false,
            Message = message
        };
} 