using Identity.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Otp.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, VerifyOtpResponse>
{
    private readonly IOtpService _otpService;
    private readonly ILogger<VerifyOtpCommandHandler> _logger;

    public VerifyOtpCommandHandler(
        IOtpService otpService,
        ILogger<VerifyOtpCommandHandler> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    public async Task<VerifyOtpResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify OTP
            bool isValid = await _otpService.VerifyOtpAsync(
                request.UserId,
                request.Code,
                request.Purpose,
                true, // Mark as used if valid
                cancellationToken);

            if (!isValid)
            {
                _logger.LogWarning("Invalid OTP provided for user {UserId} for purpose {Purpose}", 
                    request.UserId, request.Purpose);
                return VerifyOtpResponse.FailureResult("Invalid or expired OTP");
            }

            _logger.LogInformation("OTP verified successfully for user {UserId} for purpose {Purpose}", 
                request.UserId, request.Purpose);

            return VerifyOtpResponse.SuccessResult(request.UserId, request.Purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for user {UserId} for purpose {Purpose}", 
                request.UserId, request.Purpose);
            return VerifyOtpResponse.FailureResult($"Error verifying OTP: {ex.Message}");
        }
    }
} 