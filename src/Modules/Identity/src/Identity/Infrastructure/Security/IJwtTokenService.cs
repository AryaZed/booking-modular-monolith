using Identity.Domain.Aggregates.User;

namespace Identity.Infrastructure.Security;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user, long? tenantId = null);
    Task<(string token, string refreshToken)> GenerateTokenWithRefreshTokenAsync(ApplicationUser user, long? tenantId = null, string ipAddress = null, string userAgent = null);
    Task<string> RefreshTokenAsync(string refreshToken, string ipAddress = null);
    Task RevokeTokenAsync(string refreshToken, string ipAddress = null);
    Task<bool> ValidateTokenAsync(string token);
} 