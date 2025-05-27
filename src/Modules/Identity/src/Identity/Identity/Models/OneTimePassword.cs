using System;

namespace Identity.Identity.Models;

public class OneTimePassword
{
    private OneTimePassword() { }

    public long Id { get; private set; }
    public string UserId { get; private set; }
    public string Code { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public OtpPurpose Purpose { get; private set; }
    public string? Reference { get; private set; }

    public static OneTimePassword Generate(
        string userId, 
        OtpPurpose purpose, 
        TimeSpan validity, 
        string? reference = null)
    {
        return new OneTimePassword
        {
            UserId = userId,
            Code = GenerateCode(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(validity),
            IsUsed = false,
            Purpose = purpose,
            Reference = reference
        };
    }

    public bool Verify(string code)
    {
        return !IsUsed && 
               DateTime.UtcNow < ExpiresAt && 
               string.Equals(Code, code, StringComparison.Ordinal);
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
    }

    private static string GenerateCode()
    {
        // Generate a 6-digit code
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}

public enum OtpPurpose
{
    Login = 0,
    PasswordReset = 1,
    PhoneVerification = 2,
    EmailVerification = 3,
    TransactionApproval = 4,
    Other = 99
} 