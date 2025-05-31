using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BuildingBlocks.Constants;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Services;
using Identity.Data;
using Identity.Identity.Dtos;
using Identity.Identity.Exceptions;
using Identity.Identity.Models;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Services;

public class TokenService : ITokenService
{
    private readonly IOptions<IdentityServerOptions> _options;
    private readonly IKeyMaterialService _keyMaterialService;
    private readonly IdentityContext _context;
    private readonly IPermissionValidator _permissionValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    public TokenService(
        IOptions<IdentityServerOptions> options,
        IKeyMaterialService keyMaterialService,
        IdentityContext context,
        IPermissionValidator permissionValidator,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _keyMaterialService = keyMaterialService;
        _context = context;
        _permissionValidator = permissionValidator;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public async Task<TokenResponse> GenerateTokenAsync(
        ApplicationUser user,
        long? tenantId = null,
        TenantType? tenantType = null,
        long? roleId = null)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
            
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
                throw new IdentityException($"User {user.Id} does not have access to tenant {tenantId.Value}");
                
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
        
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = IdentityConstant.Auth.DefaultExpirationMinutes * 60, // convert to seconds
            TokenType = "bearer"
        };
    }
    
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.Token == refreshToken);
            
        if (token == null)
            throw new IdentityException("Invalid refresh token");
            
        if (!token.IsActive)
            throw new IdentityException("Inactive refresh token");
            
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
        
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = IdentityConstant.Auth.DefaultExpirationMinutes * 60,
            TokenType = "bearer"
        };
    }
    
    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == refreshToken);
            
        if (token == null || !token.IsActive)
            return false;
            
        // Revoke the token
        token.Revoke(GetIpAddress());
        await _context.SaveChangesAsync();
        
        return true;
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
        
        // Add module access claims for the tenant
        if (tenantId.HasValue)
        {
            // Get accessible modules for the tenant
            var moduleService = _serviceProvider.GetRequiredService<IModuleService>();
            var moduleCodes = await moduleService.GetTenantModuleCodesAsync(tenantId.Value);
            
            // Add module access claims
            foreach (var moduleCode in moduleCodes)
            {
                claims.Add(new Claim("module", moduleCode));
            }
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
        var key = _keyMaterialService.GetSigningCredentials().First();
        return key;
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
            return "unknown";
            
        var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        
        // If there's a forwarded header, use that (first address is the client)
        if (!string.IsNullOrEmpty(forwardedHeader))
            return forwardedHeader.Split(',')[0].Trim();
            
        // Otherwise use the connection remote IP
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
    
    private async Task<IEnumerable<string>> GetUserPermissionsAsync(long userId, long? tenantId)
    {
        // If tenant context is provided, get tenant-specific permissions
        if (tenantId.HasValue)
        {
            var tenantRoleIds = await _context.UserTenantRoles
                .Where(utr => utr.UserId == userId && utr.TenantId == tenantId.Value && utr.IsActive)
                .Select(utr => utr.RoleId)
                .ToListAsync();
                
            if (tenantRoleIds.Any())
            {
                var permissions = await _context.RolePermissions
                    .Where(rp => tenantRoleIds.Contains(rp.RoleId))
                    .Select(rp => rp.Permission)
                    .Distinct()
                    .ToListAsync();
                    
                return permissions;
            }
        }
        
        // If no tenant or no tenant permissions, check system permissions
        var systemRoleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();
            
        if (systemRoleIds.Any())
        {
            var permissions = await _context.RolePermissions
                .Where(rp => systemRoleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync();
                
            return permissions;
        }
        
        // If no permissions found, return empty list
        return new List<string>();
    }

    public string EncodeToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token));
            
        // Base64 URL encode the token for safe transport in URLs
        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        return Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
    
    public string DecodeToken(string encodedToken)
    {
        if (string.IsNullOrEmpty(encodedToken))
            throw new ArgumentNullException(nameof(encodedToken));
            
        // Add padding if needed
        string padded = encodedToken;
        switch (encodedToken.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        
        // Replace URL-safe characters with Base64 characters
        padded = padded.Replace('-', '+').Replace('_', '/');
        
        // Decode
        byte[] tokenBytes = Convert.FromBase64String(padded);
        return System.Text.Encoding.UTF8.GetString(tokenBytes);
    }
} 