using System.Threading.Tasks;

namespace Identity.Services;

/// <summary>
/// Service for sending application-specific emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a password reset email
    /// </summary>
    /// <param name="email">Recipient email</param>
    /// <param name="username">Recipient username</param>
    /// <param name="token">Password reset token</param>
    /// <param name="userId">User ID</param>
    Task SendPasswordResetEmailAsync(string email, string username, string token, string userId);
    
    /// <summary>
    /// Sends an email confirmation link
    /// </summary>
    /// <param name="email">Recipient email</param>
    /// <param name="username">Recipient username</param>
    /// <param name="token">Email confirmation token</param>
    Task SendEmailConfirmationAsync(string email, string username, string token);
    
    /// <summary>
    /// Sends a two-factor authentication code
    /// </summary>
    /// <param name="email">Recipient email</param>
    /// <param name="username">Recipient username</param>
    /// <param name="code">Authentication code</param>
    Task SendTwoFactorCodeAsync(string email, string username, string code);
    
    /// <summary>
    /// Sends a notification that the account has been locked
    /// </summary>
    /// <param name="email">Recipient email</param>
    /// <param name="username">Recipient username</param>
    Task SendAccountLockedEmailAsync(string email, string username);
} 