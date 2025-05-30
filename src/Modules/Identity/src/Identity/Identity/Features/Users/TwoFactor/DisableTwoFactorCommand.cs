using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Exception;
using BuildingBlocks.Security;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.TwoFactor;

/// <summary>
/// Command to disable two-factor authentication
/// </summary>
public record DisableTwoFactorCommand(string Password) : IRequest<Unit>;

public class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, Unit>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly SignInManager<Models.ApplicationUser> _signInManager;
    private readonly ILogger<DisableTwoFactorCommandHandler> _logger;

    public DisableTwoFactorCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        ICurrentUser currentUser,
        SignInManager<Models.ApplicationUser> signInManager,
        ILogger<DisableTwoFactorCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the current user
            if (!_currentUser.Id.HasValue)
            {
                _logger.LogWarning("Disable 2FA attempted with no authenticated user");
                throw new IdentityException("User not authenticated");
            }

            var userId = _currentUser.Id.Value.ToString();
            _logger.LogInformation("Processing 2FA disabling for user {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Disable 2FA attempted for non-existent user ID: {UserId}", userId);
                throw new NotFoundException($"User with ID {userId} not found");
            }

            // Check if 2FA is enabled
            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                _logger.LogWarning("2FA disabling attempted when not enabled for user {UserId}", userId);
                throw new ConflictException("Two-factor authentication is not enabled");
            }

            // Verify the password before disabling 2FA
            _logger.LogDebug("Verifying password for user {UserId}", userId);
            var isPasswordValid = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!isPasswordValid.Succeeded)
            {
                _logger.LogWarning("Invalid password provided when attempting to disable 2FA for user {UserId}", userId);
                throw new BadRequestException("Invalid password");
            }

            // Disable 2FA for the user
            _logger.LogDebug("Disabling 2FA for user {UserId}", userId);
            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to disable 2FA for user {UserId}: {Errors}", userId, errors);
                throw new InternalServerException($"Failed to disable two-factor authentication: {errors}");
            }

            // Reset the authenticator key
            _logger.LogDebug("Resetting authenticator key for user {UserId}", userId);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            _logger.LogInformation("2FA successfully disabled for user {UserId}", userId);
            return Unit.Value;
        }
        catch (Exception ex) when (
            ex is not IdentityException &&
            ex is not NotFoundException &&
            ex is not ConflictException &&
            ex is not BadRequestException &&
            ex is not InternalServerException)
        {
            _logger.LogError(ex, "Unexpected error while disabling 2FA");
            throw new InternalServerException("An unexpected error occurred while disabling two-factor authentication");
        }
    }
}
