using System.Threading.Tasks;

namespace Identity.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string username, string token, string userId);
    Task SendEmailConfirmationAsync(string email, string username, string token);
    Task SendTwoFactorCodeAsync(string email, string username, string code);
    Task SendAccountLockedEmailAsync(string email, string username);
} 