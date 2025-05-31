using System;
using System.Threading.Tasks;
using BuildingBlocks.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Services;

public class EmailService : IEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions,
        ILogger<EmailService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string username, string token, string userId)
    {
        try
        {
            var resetLink = $"/reset-password?token={token}&userId={userId}";
            var subject = "Password Reset Request";
            var body = $@"
                <h1>Password Reset</h1>
                <p>Hello {username},</p>
                <p>You have requested to reset your password. Please click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request this password reset, please ignore this email.</p>
                <p>The link will expire in 24 hours.</p>
                <p>Thank you,</p>
                <p>The Team</p>";

            await _emailSender.SendEmailAsync(email, subject, body);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
            throw;
        }
    }

    public async Task SendEmailConfirmationAsync(string email, string username, string token)
    {
        try
        {
            var confirmationLink = $"/confirm-email?token={token}&email={email}";
            var subject = "Confirm Your Email";
            var body = $@"
                <h1>Email Confirmation</h1>
                <p>Hello {username},</p>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                <p>If you did not create this account, please ignore this email.</p>
                <p>Thank you,</p>
                <p>The Team</p>";

            await _emailSender.SendEmailAsync(email, subject, body);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email confirmation to {Email}", email);
            throw;
        }
    }

    public async Task SendTwoFactorCodeAsync(string email, string username, string code)
    {
        try
        {
            var subject = "Your Two-Factor Authentication Code";
            var body = $@"
                <h1>Two-Factor Authentication Code</h1>
                <p>Hello {username},</p>
                <p>Your two-factor authentication code is: <strong>{code}</strong></p>
                <p>This code will expire in 10 minutes.</p>
                <p>If you did not request this code, please secure your account immediately.</p>
                <p>Thank you,</p>
                <p>The Team</p>";

            await _emailSender.SendEmailAsync(email, subject, body);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send 2FA code to {Email}", email);
            throw;
        }
    }

    public async Task SendAccountLockedEmailAsync(string email, string username)
    {
        try
        {
            var subject = "Account Locked";
            var body = $@"
                <h1>Account Locked</h1>
                <p>Hello {username},</p>
                <p>Your account has been temporarily locked due to multiple failed login attempts.</p>
                <p>You can try again after some time or contact support for assistance.</p>
                <p>If you did not attempt to log in, please contact support immediately as your account may be at risk.</p>
                <p>Thank you,</p>
                <p>The Team</p>";

            await _emailSender.SendEmailAsync(email, subject, body);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send account locked email to {Email}", email);
            throw;
        }
    }
} 