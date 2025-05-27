using Identity.Identity.Models;
using Identity.Identity.Features.Otp.SendOtp;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Login.TwoFactorLogin;

public class TwoFactorLoginCommandHandler : IRequestHandler<TwoFactorLoginCommand, TwoFactorLoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMediator _mediator;
    private readonly ILogger<TwoFactorLoginCommandHandler> _logger;

    public TwoFactorLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IMediator mediator,
        ILogger<TwoFactorLoginCommandHandler> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<TwoFactorLoginResponse> Handle(
        TwoFactorLoginCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Two-factor login failed: User not found for email {Email}", request.Email);
                return TwoFactorLoginResponse.FailureResult("Invalid email or password");
            }

            // Verify password
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Two-factor login failed: Invalid password for user {UserId}", user.Id);
                return TwoFactorLoginResponse.FailureResult("Invalid email or password");
            }

            // Check if user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Two-factor login failed: User {UserId} is locked out", user.Id);
                return TwoFactorLoginResponse.FailureResult("Account is locked out");
            }

            // Generate a temporary token for the 2FA flow
            var tempTokenClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("temp_token", "true"),
                new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString())
            };
            
            // Create a temporary token that expires in 10 minutes
            var tempToken = _tokenService.GenerateJwtToken(tempTokenClaims, 10);

            // Send OTP
            var sendOtpCommand = new SendOtpCommand
            {
                UserId = user.Id.ToString(),
                Purpose = OtpPurpose.Login
            };
            
            var otpResult = await _mediator.Send(sendOtpCommand, cancellationToken);
            if (!otpResult.Success)
            {
                _logger.LogError("Failed to send OTP for user {UserId}: {Message}", 
                    user.Id, otpResult.Message);
                return TwoFactorLoginResponse.FailureResult("Failed to send verification code");
            }

            _logger.LogInformation("Two-factor login initiated for user {UserId}", user.Id);
            
            return TwoFactorLoginResponse.SuccessResult(user.Id.ToString(), tempToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during two-factor login");
            return TwoFactorLoginResponse.FailureResult($"An error occurred: {ex.Message}");
        }
    }
} 