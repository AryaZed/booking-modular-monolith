using BuildingBlocks.Exception;
using Identity.Data;
using Identity.Identity.Models;
using Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Identity.Features.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IdentityContext _context;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IdentityContext context,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed for non-existent email: {Email}", request.Email);
            throw new IdentityException("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
            throw new IdentityException("Your account is currently inactive. Please contact support.");
        }

        // Validate password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out: {Email}", request.Email);
                throw new IdentityException("Your account has been locked due to too many failed login attempts. Please try again later.");
            }

            _logger.LogWarning("Invalid password for user: {Email}", request.Email);
            throw new IdentityException("Invalid email or password");
        }

        // Get user's default tenant role (if any)
        var defaultTenantRole = await _context.UserTenantRoles
            .Include(utr => utr.Tenant)
            .Include(utr => utr.Role)
            .FirstOrDefaultAsync(utr => utr.UserId == user.Id && utr.IsDefault, cancellationToken);

        // Generate tokens
        var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user, defaultTenantRole);

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Consider using configuration for expiry
        };
        await _context.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
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
            RefreshToken = refreshToken,
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