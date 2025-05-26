using System;
using BuildingBlocks.Domain.Model;

namespace Identity.Identity.Models;

public class RefreshToken : IAuditableEntity
{
    public long Id { get; private set; }
    public string Token { get; private set; }
    public long UserId { get; private set; }
    public long? TenantId { get; private set; }
    public TenantType? TenantType { get; private set; }
    public long? RoleId { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string ReplacedByToken { get; private set; }
    public string CreatedByIp { get; private set; }
    public string RevokedByIp { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public long? CreatedBy { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public long? LastModifiedBy { get; private set; }
    
    // Navigation property
    public virtual ApplicationUser User { get; private set; }
    
    // Domain methods
    public bool IsActive => !IsRevoked && ExpiryDate > DateTime.UtcNow;
    
    public void Revoke(string ipAddress, string replacementToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReplacedByToken = replacementToken;
    }
    
    // Factory method
    public static RefreshToken Generate(
        long userId, 
        long? tenantId, 
        TenantType? tenantType, 
        long? roleId, 
        string ipAddress,
        int expiryDays = 7)
    {
        return new RefreshToken
        {
            Token = GenerateToken(),
            UserId = userId,
            TenantId = tenantId,
            TenantType = tenantType,
            RoleId = roleId,
            ExpiryDate = DateTime.UtcNow.AddDays(expiryDays),
            CreatedByIp = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    private static string GenerateToken()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
} 