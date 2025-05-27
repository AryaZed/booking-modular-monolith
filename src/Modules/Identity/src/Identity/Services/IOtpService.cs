using Identity.Identity.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Services;

public interface IOtpService
{
    /// <summary>
    /// Generates a new OTP for the specified user and purpose
    /// </summary>
    Task<string> GenerateOtpAsync(
        string userId, 
        OtpPurpose purpose, 
        TimeSpan? validity = null, 
        string? reference = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifies an OTP for the specified user and purpose
    /// </summary>
    Task<bool> VerifyOtpAsync(
        string userId, 
        string code, 
        OtpPurpose purpose, 
        bool markAsUsed = true, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the most recent valid OTP for the specified user and purpose
    /// </summary>
    Task<OneTimePassword?> GetValidOtpAsync(
        string userId, 
        OtpPurpose purpose, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidates all existing OTPs for the specified user and purpose
    /// </summary>
    Task InvalidateOtpsAsync(
        string userId, 
        OtpPurpose purpose, 
        CancellationToken cancellationToken = default);
} 