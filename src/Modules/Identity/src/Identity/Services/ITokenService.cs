using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Identity.Dtos;
using Identity.Identity.Models;

namespace Identity.Services;

public interface ITokenService
{
    Task<TokenResponse> GenerateTokenAsync(
        ApplicationUser user,
        long? tenantId = null,
        TenantType? tenantType = null,
        long? roleId = null);
        
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    
    Task<bool> RevokeTokenAsync(string refreshToken);
    
    // Legacy method for backward compatibility
    string GenerateToken(
        string userId,
        string username,
        string email,
        string tenantId,
        string tenantType,
        string roleName,
        IEnumerable<string> permissions);

    string EncodeToken(string token);
    string DecodeToken(string encodedToken);
} 