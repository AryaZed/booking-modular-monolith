using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.Security;
using FluentValidation;
using Identity.Identity.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.TwoFactor;

public record VerifyTwoFactorCommand(string Code) : ICommand;

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

public class VerifyTwoFactorCommandHandler : ICommandHandler<VerifyTwoFactorCommand>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

    public VerifyTwoFactorCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        ICurrentUser currentUser,
        ILogger<VerifyTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Unit> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        // Get the current user
        var userId = _currentUser.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            _logger.LogWarning("Verify 2FA attempted for non-existent user ID: {UserId}", userId);
            throw new IdentityException("User not found");
        }

        // Check if 2FA is already enabled
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            _logger.LogWarning("2FA verification attempted when already enabled for user {UserId}", userId);
            throw new IdentityException("Two-factor authentication is already enabled");
        }

        // Verify the code
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, 
            _userManager.Options.Tokens.AuthenticatorTokenProvider, 
            request.Code);

        if (!isValid)
        {
            _logger.LogWarning("Invalid 2FA verification code for user {UserId}", userId);
            throw new IdentityException("Verification code is invalid");
        }

        // Enable 2FA for the user
        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to enable 2FA for user {UserId}: {Errors}", userId, errors);
            throw new IdentityException($"Failed to enable two-factor authentication: {errors}");
        }

        _logger.LogInformation("2FA successfully enabled for user {UserId}", userId);
        return Unit.Value;
    }
} 