using Identity.Identity.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Services;

public interface IOtpNotificationService
{
    /// <summary>
    /// Sends an OTP notification to the user (SMS, Email, etc.)
    /// </summary>
    Task SendOtpAsync(
        string userId, 
        string code, 
        OtpPurpose purpose, 
        CancellationToken cancellationToken = default);
} 