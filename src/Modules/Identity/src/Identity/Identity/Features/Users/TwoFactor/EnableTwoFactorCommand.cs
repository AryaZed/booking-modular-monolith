using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Security;
using Identity.Identity.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.TwoFactor;

public record EnableTwoFactorCommand() : ICommand<EnableTwoFactorResponse>;

public record EnableTwoFactorResponse(string RecoveryCodesBase64, string SharedKey, string AuthenticatorUri);

public class EnableTwoFactorCommandHandler : ICommandHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<EnableTwoFactorCommandHandler> _logger;

    public EnableTwoFactorCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        ICurrentUser currentUser,
        ILogger<EnableTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<EnableTwoFactorResponse> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        // Get the current user
        var userId = _currentUser.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            _logger.LogWarning("Enable 2FA attempted for non-existent user ID: {UserId}", userId);
            throw new IdentityException("User not found");
        }

        // Check if 2FA is already enabled
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            _logger.LogWarning("2FA is already enabled for user {UserId}", userId);
            throw new IdentityException("Two-factor authentication is already enabled");
        }

        // Generate the shared key and QR code URI
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var email = await _userManager.GetEmailAsync(user);
        var authenticatorUri = GenerateQrCodeUri(email, unformattedKey);

        // Generate recovery codes
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        var recoveryCodesBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Join(",", recoveryCodes)));

        _logger.LogInformation("2FA setup initiated for user {UserId}", userId);

        return new EnableTwoFactorResponse(
            RecoveryCodesBase64: recoveryCodesBase64,
            SharedKey: unformattedKey,
            AuthenticatorUri: authenticatorUri);
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        const string authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        return string.Format(
            authenticatorUriFormat,
            "BookingApp",
            Uri.EscapeDataString(email),
            unformattedKey);
    }
} 