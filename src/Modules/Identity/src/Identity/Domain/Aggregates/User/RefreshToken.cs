using BuildingBlocks.Domain.Model;

namespace Identity.Domain.Aggregates.User;

public class RefreshToken : Entity
{
    public string Token { get; private set; }
    public long UserId { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public string ReplacedByToken { get; private set; }
    public string CreatedByIp { get; private set; }
    public string RevokedByIp { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string UserAgent { get; private set; }

    // Navigation properties
    public ApplicationUser User { get; private set; }

    private RefreshToken()
    {
        // Required by EF Core
    }

    public static RefreshToken Create(ApplicationUser user, string token, DateTime expiryDate, string createdByIp, string userAgent)
    {
        return new RefreshToken
        {
            Token = token,
            UserId = user.Id,
            User = user,
            ExpiryDate = expiryDate,
            CreatedByIp = createdByIp,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string replacedByToken = null, string revokedByIp = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
        RevokedByIp = revokedByIp;
    }
} 