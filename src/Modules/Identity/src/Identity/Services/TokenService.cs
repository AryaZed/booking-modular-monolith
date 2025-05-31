using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BuildingBlocks.Constants;
using BuildingBlocks.Exception;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Services;
using Identity.Data;
using Identity.Identity.Dtos;
using Identity.Identity.Models;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Services;

/// <summary>
/// Service for managing authentication tokens including JWT generation, refresh tokens, and token encoding/decoding
/// </summary>
public class TokenService : ITokenService
{
    private readonly IOptions<IdentityServerOptions> _options;
    private readonly IKeyMaterialService _keyMaterialService;
    private readonly IdentityContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenService(
        IOptions<IdentityServerOptions> options,
        IKeyMaterialService keyMaterialService,
        IdentityContext context,
        IPermissionValidator permissionValidator,
        IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _keyMaterialService = keyMaterialService;
        _context = context;
        _permissionValidator = permissionValidator;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public async Task<TokenResponse> GenerateTokenAsync(
        ApplicationUser user,
        long? tenantId = null,
        TenantType? tenantType = null,
        long? roleId = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            _logger.LogDebug(
                "Generating token for user {UserId} with tenant context: {TenantId}, {TenantType}",
                user.Id, tenantId, tenantType);

            // If tenant context is provided, validate the user has access to this tenant
            UserTenantRole userTenantRole = null;
            string roleName = IdentityConstant.Role.User; // Default role

            if (tenantId.HasValue)
            {
                userTenantRole = await _context.UserTenantRoles
                    .Include(utr => utr.Role)
                    .FirstOrDefaultAsync(utr =>
                        utr.UserId == user.Id &&
                        utr.TenantId == tenantId.Value &&
                        (tenantType == null || utr.Tenant.Type == tenantType.Value) &&
                        (roleId == null || utr.RoleId == roleId.Value) &&
                        utr.IsActive);

                if (userTenantRole == null)
                {
                    _logger.LogWarning("User {UserId} does not have access to tenant {TenantId}", user.Id, tenantId.Value);
                    throw new BadRequestException($"User does not have access to the specified tenant");
                }

                roleName = userTenantRole.Role.Name;
            }

            // Get permissions for this user
            var permissions = await GetUserPermissionsAsync(user.Id, tenantId);

            // Generate JWT token
            var accessToken = GenerateJwtToken(
                user.Id.ToString(),
                user.UserName,
                user.Email,
                tenantId?.ToString(),
                tenantType?.ToString(),
                roleName,
                permissions);

            // Generate refresh token
            var refreshToken = CreateRefreshToken(user.Id, tenantId, tenantType, roleId);

            // Save refresh token to database
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Token generated successfully for user {UserId}", user.Id);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = IdentityConstant.Auth.DefaultExpirationMinutes * 60, // convert to seconds
                TokenType = "bearer"
            };
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            _logger.LogError(ex, "Error generating token for user {UserId}", user.Id);
            throw new InternalServerException("An error occurred while generating the authentication token");
        }
    }

    /// <inheritdoc/>
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentNullException(nameof(refreshToken));
        }

        try
        {
            _logger.LogDebug("Processing refresh token request");

            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .SingleOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
            {
                _logger.LogWarning("Invalid refresh token provided");
                throw new BadRequestException("Invalid refresh token");
            }

            if (!token.IsActive)
            {
                _logger.LogWarning("Inactive refresh token provided for user {UserId}", token.UserId);
                throw new BadRequestException("Refresh token has expired or been revoked");
            }

            // Generate new refresh token to replace the old one
            var newRefreshToken = CreateRefreshToken(
                token.UserId,
                token.TenantId,
                token.TenantType,
                token.RoleId);

            // Revoke the current refresh token
            token.Revoke(GetIpAddress(), newRefreshToken.Token);

            // Add the new refresh token
            _context.RefreshTokens.Add(newRefreshToken);

            // Get the user and tenant info
            var user = token.User;
            if (user == null)
            {
                _logger.LogWarning("User {UserId} associated with refresh token not found", token.UserId);
                throw new NotFoundException("User associated with refresh token not found");
            }

            // Get permissions
            var permissions = await GetUserPermissionsAsync(user.Id, token.TenantId);

            // Get role name
            string roleName = IdentityConstant.Role.User;
            if (token.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(token.RoleId.Value);
                if (role != null)
                    roleName = role.Name;
            }

            // Generate new JWT token
            var accessToken = GenerateJwtToken(
                user.Id.ToString(),
                user.UserName,
                user.Email,
                token.TenantId?.ToString(),
                token.TenantType?.ToString(),
                roleName,
                permissions);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresIn = IdentityConstant.Auth.DefaultExpirationMinutes * 60,
                TokenType = "bearer"
            };
        }
        catch (Exception ex) when (ex is not BadRequestException && ex is not NotFoundException)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw new InternalServerException("An error occurred while refreshing the authentication token");
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentNullException(nameof(refreshToken));
        }

        try
        {
            _logger.LogDebug("Revoking refresh token");

            var token = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
            {
                _logger.LogWarning("Attempted to revoke non-existent refresh token");
                return false;
            }

            if (!token.IsActive)
            {
                _logger.LogInformation("Attempted to revoke already inactive refresh token for user {UserId}", token.UserId);
                return false;
            }

            // Revoke the token
            token.Revoke(GetIpAddress());
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked successfully for user {UserId}", token.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            throw new InternalServerException("An error occurred while revoking the refresh token");
        }
    }

    /// <inheritdoc/>
    public string GenerateJwtToken(
        string userId,
        string username,
        string email,
        string tenantId,
        string tenantType,
        string roleName,
        IEnumerable<string> permissions)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentNullException(nameof(email));
        }

        try
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, userId),
                new Claim(JwtClaimTypes.Name, username),
                new Claim(JwtClaimTypes.Email, email),
                new Claim(JwtClaimTypes.Role, roleName),
                new Claim(JwtClaimTypes.JwtId, Guid.NewGuid().ToString("N")),
                new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add tenant claims if available
            if (!string.IsNullOrEmpty(tenantId))
            {
                claims.Add(new Claim(IdentityConstant.ClaimTypes.TenantId, tenantId));
            }

            if (!string.IsNullOrEmpty(tenantType))
            {
                claims.Add(new Claim(IdentityConstant.ClaimTypes.TenantType, tenantType));
            }

            // Add permission claims
            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    if (!string.IsNullOrEmpty(permission))
                    {
                        claims.Add(new Claim(IdentityConstant.ClaimTypes.Permission, permission));
                    }
                }
            }

            var signingCredentials = GetSigningCredentials();
            var expires = DateTime.UtcNow.AddMinutes(IdentityConstant.Auth.DefaultExpirationMinutes);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _options.Value.IssuerUri,
                Audience = "booking",
                SigningCredentials = signingCredentials,
                NotBefore = DateTime.UtcNow
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {UserId}", userId);
            throw new InternalServerException("An error occurred while generating the JWT token");
        }
    }

    // Legacy method for backward compatibility
    public string GenerateToken(
        string userId,
        string username,
        string email,
        string tenantId,
        string tenantType,
        string roleName,
        IEnumerable<string> permissions)
    {
        return GenerateJwtToken(userId, username, email, tenantId, tenantType, roleName, permissions);
    }
    
    private string GenerateJwtToken(
        string userId,
        string username,
        string email,
        string tenantId,
        string tenantType,
        string roleName,
        IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtClaimTypes.Subject, userId),
            new Claim(JwtClaimTypes.Name, username),
            new Claim(JwtClaimTypes.Email, email),
            new Claim(JwtClaimTypes.Role, roleName)
        };
        
        // Add tenant claims if available
        if (!string.IsNullOrEmpty(tenantId))
            claims.Add(new Claim(IdentityConstant.ClaimTypes.TenantId, tenantId));
            
        if (!string.IsNullOrEmpty(tenantType))
            claims.Add(new Claim(IdentityConstant.ClaimTypes.TenantType, tenantType));
        
        // Add permission claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim(IdentityConstant.ClaimTypes.Permission, permission));
        }
        
        var signingCredentials = GetSigningCredentials();
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(IdentityConstant.Auth.DefaultExpirationMinutes),
            Issuer = _options.Value.IssuerUri,
            Audience = "booking",
            SigningCredentials = signingCredentials
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private SigningCredentials GetSigningCredentials()
    {
        try
        {
            return _keyMaterialService.GetSigningCredentialsAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signing credentials");
            throw new InternalServerException("An error occurred while retrieving signing credentials");
        }
    }

    private RefreshToken CreateRefreshToken(
        long userId,
        long? tenantId,
        TenantType? tenantType,
        long? roleId)
    {
        return RefreshToken.Generate(
            userId,
            tenantId,
            tenantType,
            roleId,
            GetIpAddress(),
            IdentityConstant.Auth.RefreshTokenExpirationDays);
    }

    private string GetIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return "unknown";
        }

        // Check for forwarded header first (for clients behind proxies)
        var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            // Get the first IP which is the client IP
            return forwardedHeader.Split(',')[0].Trim();
        }

        // Fall back to connection remote IP
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private async Task<IEnumerable<string>> GetUserPermissionsAsync(long userId, long? tenantId)
    {
        try
        {
            _logger.LogDebug("Getting permissions for user {UserId} with tenant context {TenantId}", userId, tenantId);

            // If tenant context is provided, get tenant-specific permissions
            if (tenantId.HasValue)
            {
                var permissions = await _permissionService.GetUserPermissionsForTenantAsync(userId, tenantId.Value);
                return permissions;
            }

            // If no tenant context, get all user permissions
            var allPermissions = await _permissionService.GetUserPermissionsAsync(userId);
            return allPermissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
            throw new InternalServerException("An error occurred while retrieving user permissions");
        }
    }
}
