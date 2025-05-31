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
/// Command to verify and enable two-factor authentication
/// </summary>
public record VerifyTwoFactorCommand(string Code) : IRequest<Unit>;

public class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required")
            .Length(6).WithMessage("Verification code must be 6 digits")
            .Matches("^[0-9]+$").WithMessage("Verification code must contain only digits");
    }
}

public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, Unit>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

    public VerifyTwoFactorCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        ICurrentUser currentUser,
        ILogger<VerifyTwoFactorCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the current user
            if (!_currentUser.Id.HasValue)
            {
                _logger.LogWarning("Verify 2FA attempted with no authenticated user");
                throw new IdentityException("User not authenticated");
            }

            var userId = _currentUser.Id.Value.ToString();
            _logger.LogInformation("Processing 2FA verification for user {UserId}", userId);
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Verify 2FA attempted for non-existent user ID: {UserId}", userId);
                throw new NotFoundException($"User with ID {userId} not found");
            }

            // Check if 2FA is already enabled
            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                _logger.LogWarning("2FA verification attempted when already enabled for user {UserId}", userId);
                throw new ConflictException("Two-factor authentication is already enabled");
            }

            // Verify the code
            _logger.LogDebug("Verifying 2FA code for user {UserId}", userId);
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                request.Code);

            if (!isValid)
            {
                _logger.LogWarning("Invalid 2FA verification code for user {UserId}", userId);
                throw new BadRequestException("Verification code is invalid");
            }

            // Enable 2FA for the user
            _logger.LogDebug("Enabling 2FA for user {UserId}", userId);
            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to enable 2FA for user {UserId}: {Errors}", userId, errors);
                throw new InternalServerException($"Failed to enable two-factor authentication: {errors}");
            }

            _logger.LogInformation("2FA successfully enabled for user {UserId}", userId);
            return Unit.Value;
        }
        catch (Exception ex) when (
            ex is not IdentityException && 
            ex is not NotFoundException && 
            ex is not ConflictException && 
            ex is not BadRequestException &&
            ex is not InternalServerException)
        {
            _logger.LogError(ex, "Unexpected error while verifying 2FA");
            throw new InternalServerException("An unexpected error occurred while verifying two-factor authentication");
        }
    }
}
