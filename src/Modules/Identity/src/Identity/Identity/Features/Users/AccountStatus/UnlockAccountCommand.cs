using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using FluentValidation;
using Identity.Identity.Exceptions;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.AccountStatus;

public record UnlockAccountCommand(string UserId) : ICommand;

public class UnlockAccountCommandValidator : AbstractValidator<UnlockAccountCommand>
{
    public UnlockAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}

public class UnlockAccountCommandHandler : ICommandHandler<UnlockAccountCommand>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<UnlockAccountCommandHandler> _logger;

    public UnlockAccountCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<UnlockAccountCommandHandler> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Unit> Handle(UnlockAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("Account unlock attempted for non-existent user ID: {UserId}", request.UserId);
            throw new IdentityException("User not found");
        }

        // Check if account is locked
        if (!await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Account unlock attempted for non-locked user ID: {UserId}", request.UserId);
            throw new IdentityException("Account is not locked");
        }

        // Reset the lockout and access failed count
        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to unlock account for user {UserId}: {Errors}", user.Id, errors);
            throw new IdentityException($"Failed to unlock account: {errors}");
        }

        // Reset the access failed count
        await _userManager.ResetAccessFailedCountAsync(user);

        // Notify the user via email
        try
        {
            await _emailService.SendAccountLockedEmailAsync(user.Email, user.UserName);
        }
        catch (Exception ex)
        {
            // Log but don't fail the operation if email sending fails
            _logger.LogError(ex, "Failed to send account unlock notification email to user {UserId}", user.Id);
        }

        _logger.LogInformation("Account successfully unlocked for user {UserId}", user.Id);
        return Unit.Value;
    }
} 