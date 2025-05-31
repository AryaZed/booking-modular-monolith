using BuildingBlocks.Domain.Model;

namespace Identity.Domain.Aggregates.User;

public class OneTimePassword : Entity
{
    public long UserId { get; private set; }
    public string Code { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public bool IsUsed { get; private set; }
    public OtpType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    // Navigation properties
    public ApplicationUser User { get; private set; }

    private OneTimePassword()
    {
        // Required by EF Core
    }

    public static OneTimePassword Create(ApplicationUser user, string code, OtpType type, TimeSpan expiryTime)
    {
        return new OneTimePassword
        {
            UserId = user.Id,
            User = user,
            Code = code,
            Type = type,
            ExpiryDate = DateTime.UtcNow.Add(expiryTime),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsValid => !IsUsed && !IsExpired;

    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}

public enum OtpType
{
    TwoFactorAuth = 0,
    PasswordReset = 1,
    EmailVerification = 2,
    PhoneVerification = 3
} 