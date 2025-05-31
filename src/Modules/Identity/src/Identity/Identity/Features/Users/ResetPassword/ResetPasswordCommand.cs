using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Exception;
using FluentValidation;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Users.ResetPassword;

/// <summary>
/// Command to reset a user's password using a reset token
/// </summary>
public record ResetPasswordCommand(string UserId, string Token, string NewPassword, string ConfirmPassword) : IRequest<Unit>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least 1 lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least 1 special character");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly UserManager<Models.ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<Models.ApplicationUser> userManager,
        ITokenService tokenService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing password reset request for user ID: {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("Password reset attempted for non-existent user ID: {UserId}", request.UserId);
                throw new NotFoundException("User not found");
            }

            // Decode the token
            _logger.LogDebug("Decoding password reset token for user {UserId}", user.Id);
            string decodedToken;

            try
            {
                decodedToken = _tokenService.DecodeToken(request.Token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid reset token provided for user {UserId}", user.Id);
                throw new BadRequestException("Invalid password reset token");
            }

            // Reset the password
            _logger.LogDebug("Resetting password for user {UserId}", user.Id);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errors);
                throw new BadRequestException($"Password reset failed: {errors}");
            }

            _logger.LogInformation("Password reset successful for user {UserId}", user.Id);
            return Unit.Value;
        }
        catch (Exception ex) when (
            ex is not NotFoundException &&
            ex is not BadRequestException &&
            ex is not InternalServerException)
        {
            _logger.LogError(ex, "Unexpected error during password reset");
            throw new InternalServerException("An unexpected error occurred during password reset");
        }
    }
}
