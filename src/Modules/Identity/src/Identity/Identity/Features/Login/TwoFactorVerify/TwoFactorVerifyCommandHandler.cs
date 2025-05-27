using BuildingBlocks.Constants;
using Identity.Identity.Models;
using Identity.Identity.Features.Otp.VerifyOtp;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Login.TwoFactorVerify;

public class TwoFactorVerifyCommandHandler : IRequestHandler<TwoFactorVerifyCommand, TwoFactorVerifyResponse>
{
    private readonly IMediator _mediator;
    private readonly ITokenService _tokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TwoFactorVerifyCommandHandler> _logger;

    public TwoFactorVerifyCommandHandler(
        IMediator mediator,
        ITokenService tokenService,
        UserManager<ApplicationUser> userManager,
        ILogger<TwoFactorVerifyCommandHandler> logger)
    {
        _mediator = mediator;
        _tokenService = tokenService;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<TwoFactorVerifyResponse> Handle(
        TwoFactorVerifyCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate the temporary token
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(request.TempToken) as JwtSecurityToken;
            
            if (jsonToken == null)
            {
                _logger.LogWarning("Invalid temporary token format");
                return TwoFactorVerifyResponse.FailureResult("Invalid token");
            }
            
            // Check token is a temp token
            var isTempToken = jsonToken.Claims
                .FirstOrDefault(c => c.Type == "temp_token")?.Value == "true";
                
            if (!isTempToken)
            {
                _logger.LogWarning("Token is not a temporary token");
                return TwoFactorVerifyResponse.FailureResult("Invalid token type");
            }
            
            // Extract and validate user ID
            var tokenUserId = jsonToken.Claims
                .FirstOrDefault(c => c.Type == "nameid" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                ?.Value;
                
            if (string.IsNullOrEmpty(tokenUserId) || tokenUserId != request.UserId)
            {
                _logger.LogWarning("User ID mismatch or missing in token");
                return TwoFactorVerifyResponse.FailureResult("Invalid token");
            }
            
            // Verify OTP
            var verifyOtpCommand = new VerifyOtpCommand
            {
                UserId = request.UserId,
                Code = request.OtpCode,
                Purpose = OtpPurpose.Login
            };
            
            var otpResult = await _mediator.Send(verifyOtpCommand, cancellationToken);
            if (!otpResult.Success)
            {
                _logger.LogWarning("OTP verification failed for user {UserId}: {Message}", 
                    request.UserId, otpResult.Message);
                return TwoFactorVerifyResponse.FailureResult("Invalid verification code");
            }
            
            // Get the user
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", request.UserId);
                return TwoFactorVerifyResponse.FailureResult("User not found");
            }
            
            // Reset failed access attempts
            if (user.AccessFailedCount > 0)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            // Parse tenant type if provided
            TenantType? tenantType = null;
            if (!string.IsNullOrEmpty(request.TenantType) && 
                Enum.TryParse<TenantType>(request.TenantType, out var parsedTenantType))
            {
                tenantType = parsedTenantType;
            }
            
            // Generate tokens
            var tokenResult = await _tokenService.GenerateTokensAsync(
                user.Id.ToString(),
                user.UserName,
                request.TenantId,
                tenantType);
                
            _logger.LogInformation("Two-factor verification successful for user {UserId}", request.UserId);
                
            return TwoFactorVerifyResponse.SuccessResult(
                request.UserId,
                tokenResult.AccessToken,
                tokenResult.RefreshToken,
                tokenResult.ExpiresIn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during two-factor verification");
            return TwoFactorVerifyResponse.FailureResult($"An error occurred: {ex.Message}");
        }
    }
} 