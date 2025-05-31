using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Exception;
using BuildingBlocks.Security;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.TwoFactor;

/// <summary>
/// Command to initialize two-factor authentication setup
/// </summary>
public record EnableTwoFactorCommand : IRequest<EnableTwoFactorResponse>;

/// <summary>
/// Response containing data needed for two-factor authentication setup
/// </summary>
public record EnableTwoFactorResponse(string RecoveryCodesBase64, string SharedKey, string AuthenticatorUri)
{
    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static EnableTwoFactorResponse Success(string recoveryCodesBase64, string sharedKey, string authenticatorUri)
        => new(recoveryCodesBase64, sharedKey, authenticatorUri);
}

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<EnableTwoFactorCommandHandler> _logger;

    public EnableTwoFactorCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        ICurrentUser currentUser,
        ILogger<EnableTwoFactorCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EnableTwoFactorResponse> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the current user
            if (!_currentUser.Id.HasValue)
            {
                _logger.LogWarning("Enable 2FA attempted with no authenticated user");
                throw new IdentityException("User not authenticated");
            }

            var userId = _currentUser.Id.Value.ToString();
            _logger.LogInformation("Initializing 2FA setup for user {UserId}", userId);
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Enable 2FA attempted for non-existent user ID: {UserId}", userId);
                throw new NotFoundException($"User with ID {userId} not found");
            }

            // Check if 2FA is already enabled
            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                _logger.LogWarning("2FA setup attempted when already enabled for user {UserId}", userId);
                throw new ConflictException("Two-factor authentication is already enabled");
            }

            // Generate the shared key and QR code URI
            _logger.LogDebug("Generating authenticator key for user {UserId}", userId);
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                _logger.LogDebug("Resetting authenticator key for user {UserId}", userId);
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = await _userManager.GetEmailAsync(user);
            var authenticatorUri = GenerateQrCodeUri(email, unformattedKey);

            // Generate recovery codes
            _logger.LogDebug("Generating recovery codes for user {UserId}", userId);
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            var recoveryCodesBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Join(",", recoveryCodes)));

            _logger.LogInformation("2FA setup initiated for user {UserId}", userId);

            return EnableTwoFactorResponse.Success(
                recoveryCodesBase64: recoveryCodesBase64,
                sharedKey: unformattedKey,
                authenticatorUri: authenticatorUri);
        }
        catch (Exception ex) when (
            ex is not IdentityException &&
            ex is not NotFoundException &&
            ex is not ConflictException)
        {
            _logger.LogError(ex, "Unexpected error while setting up 2FA");
            throw new InternalServerException("An unexpected error occurred while setting up two-factor authentication");
        }
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
