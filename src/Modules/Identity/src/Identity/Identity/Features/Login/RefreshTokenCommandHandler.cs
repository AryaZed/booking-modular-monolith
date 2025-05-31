using BuildingBlocks.Exception;
using Identity.Data;
using Identity.Identity.Models;
using Identity.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Login;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IdentityContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IdentityContext context,
        ITokenService tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the refresh token
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken);
            throw new IdentityException("Invalid refresh token");
        }

        // Check if the token is expired
        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired refresh token used: {Token}, Expired: {ExpiredAt}", request.RefreshToken, refreshToken.ExpiresAt);
            
            // Remove expired token
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            throw new IdentityException("Refresh token has expired");
        }

        var user = refreshToken.User;
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Refresh token used for inactive or deleted user: {UserId}", refreshToken.UserId);
            throw new IdentityException("User account is inactive or has been deleted");
        }

        // Get user's default tenant role (if any)
        var defaultTenantRole = await _context.UserTenantRoles
            .Include(utr => utr.Tenant)
            .Include(utr => utr.Role)
            .FirstOrDefaultAsync(utr => utr.UserId == user.Id && utr.IsDefault, cancellationToken);

        // Generate new tokens
        var (accessToken, newRefreshToken) = await _tokenService.GenerateTokensAsync(user, defaultTenantRole);

        // Update refresh token
        refreshToken.Token = newRefreshToken;
        refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7); // Consider using configuration for expiry
        
        await _context.SaveChangesAsync(cancellationToken);

        // Get permissions for the role
        IEnumerable<string> permissions = new List<string>();
        if (defaultTenantRole != null)
        {
            permissions = await _tokenService.GetPermissionsForRoleAsync(defaultTenantRole.RoleId);
        }

        // Create response
        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CurrentTenantId = defaultTenantRole?.TenantId,
            CurrentTenantName = defaultTenantRole?.Tenant?.Name,
            CurrentRole = defaultTenantRole?.Role?.Name,
            Permissions = permissions,
            ExpiresIn = 3600 // 1 hour in seconds - should match token lifetime in identity configuration
        };
    }
} 