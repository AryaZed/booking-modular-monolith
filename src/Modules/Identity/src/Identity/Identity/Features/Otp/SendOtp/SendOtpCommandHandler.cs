using Identity.Identity.Models;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Otp.SendOtp;

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, SendOtpResponse>
{
    private readonly IOtpService _otpService;
    private readonly IOtpNotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SendOtpCommandHandler> _logger;

    public SendOtpCommandHandler(
        IOtpService otpService,
        IOtpNotificationService notificationService,
        UserManager<ApplicationUser> userManager,
        ILogger<SendOtpCommandHandler> logger)
    {
        _otpService = otpService;
        _notificationService = notificationService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<SendOtpResponse> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate user exists
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Cannot send OTP: User {UserId} not found", request.UserId);
                return SendOtpResponse.FailureResult("User not found");
            }

            // Generate OTP
            var code = await _otpService.GenerateOtpAsync(
                request.UserId,
                request.Purpose,
                null, // Use default validity
                request.Reference,
                cancellationToken);

            // Send notification
            await _notificationService.SendOtpAsync(
                request.UserId,
                code,
                request.Purpose,
                cancellationToken);

            _logger.LogInformation("OTP sent for user {UserId} for purpose {Purpose}", request.UserId, request.Purpose);

            return SendOtpResponse.SuccessResult(request.UserId, request.Purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP for user {UserId} for purpose {Purpose}", 
                request.UserId, request.Purpose);
            return SendOtpResponse.FailureResult($"Error sending OTP: {ex.Message}");
        }
    }
} 