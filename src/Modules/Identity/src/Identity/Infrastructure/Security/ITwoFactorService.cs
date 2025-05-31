using Identity.Domain.Aggregates.User;

namespace Identity.Infrastructure.Security;

public interface ITwoFactorService
{
    Task<string> GenerateOtpAsync(ApplicationUser user, OtpType type);
    Task<bool> ValidateOtpAsync(ApplicationUser user, string code, OtpType type);
    Task<string> GenerateQrCodeUriAsync(ApplicationUser user, string appName = "IdentityApp");
    Task<bool> SetupTwoFactorAsync(ApplicationUser user, string verificationCode);
    Task<bool> DisableTwoFactorAsync(ApplicationUser user, string verificationCode);
} 