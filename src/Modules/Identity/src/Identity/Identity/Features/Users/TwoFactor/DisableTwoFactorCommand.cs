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

public record DisableTwoFactorCommand(string Password) : ICommand;

public class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class DisableTwoFactorCommandHandler : ICommandHandler<DisableTwoFactorCommand>
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
        _userManager = userManager;
        _currentUser = currentUser;
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<Unit> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        // Get the current user
        var userId = _currentUser.GetUserId();
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            _logger.LogWarning("Disable 2FA attempted for non-existent user ID: {UserId}", userId);
            throw new IdentityException("User not found");
        }

        // Check if 2FA is enabled
        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            _logger.LogWarning("2FA disabling attempted when not enabled for user {UserId}", userId);
            throw new IdentityException("Two-factor authentication is not enabled");
        }

        // Verify the password before disabling 2FA
        var isPasswordValid = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!isPasswordValid.Succeeded)
        {
            _logger.LogWarning("Invalid password provided when attempting to disable 2FA for user {UserId}", userId);
            throw new IdentityException("Invalid password");
        }

        // Disable 2FA for the user
        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to disable 2FA for user {UserId}: {Errors}", userId, errors);
            throw new IdentityException($"Failed to disable two-factor authentication: {errors}");
        }

        // Reset the authenticator key
        await _userManager.ResetAuthenticatorKeyAsync(user);

        _logger.LogInformation("2FA successfully disabled for user {UserId}", userId);
        return Unit.Value;
    }
} 