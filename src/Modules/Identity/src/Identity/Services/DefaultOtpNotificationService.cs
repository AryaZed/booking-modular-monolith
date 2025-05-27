using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Services;

/// <summary>
/// Default implementation of IOtpNotificationService that logs the OTP 
/// and would integrate with SMS or email services in a production environment
/// </summary>
public class DefaultOtpNotificationService : IOtpNotificationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DefaultOtpNotificationService> _logger;

    public DefaultOtpNotificationService(
        UserManager<ApplicationUser> userManager,
        ILogger<DefaultOtpNotificationService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SendOtpAsync(
        string userId, 
        string code, 
        OtpPurpose purpose, 
        CancellationToken cancellationToken = default)
    {
        // Get the user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Cannot send OTP notification: User {UserId} not found", userId);
            return;
        }

        // Log the OTP (for development/demo purposes)
        _logger.LogInformation(
            "OTP for user {Email} ({UserId}): {Code} - Purpose: {Purpose}",
            user.Email, userId, code, purpose);

        // In a production environment, you would send an SMS or email here
        // Example SMS implementation:
        // await _smsService.SendSmsAsync(user.PhoneNumber, $"Your verification code is {code}");
        
        // Example Email implementation:
        // var message = new EmailMessage
        // {
        //     To = user.Email,
        //     Subject = GetSubjectForPurpose(purpose),
        //     Body = GetBodyForPurpose(purpose, code)
        // };
        // await _emailService.SendEmailAsync(message);
    }

    private string GetSubjectForPurpose(OtpPurpose purpose)
    {
        return purpose switch
        {
            OtpPurpose.Login => "Your login verification code",
            OtpPurpose.PasswordReset => "Your password reset code",
            OtpPurpose.EmailVerification => "Verify your email address",
            OtpPurpose.PhoneVerification => "Verify your phone number",
            OtpPurpose.TransactionApproval => "Transaction approval code",
            _ => "Your verification code"
        };
    }

    private string GetBodyForPurpose(OtpPurpose purpose, string code)
    {
        return purpose switch
        {
            OtpPurpose.Login => $"Your login verification code is {code}. It will expire in 10 minutes.",
            OtpPurpose.PasswordReset => $"Your password reset code is {code}. It will expire in 10 minutes.",
            OtpPurpose.EmailVerification => $"Your email verification code is {code}. It will expire in 10 minutes.",
            OtpPurpose.PhoneVerification => $"Your phone verification code is {code}. It will expire in 10 minutes.",
            OtpPurpose.TransactionApproval => $"Your transaction approval code is {code}. It will expire in 10 minutes.",
            _ => $"Your verification code is {code}. It will expire in 10 minutes."
        };
    }
} 