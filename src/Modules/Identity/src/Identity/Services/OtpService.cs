using Identity.Data;
using Identity.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Services;

public class OtpService : IOtpService
{
    private readonly IdentityContext _context;
    private readonly ILogger<OtpService> _logger;
    private static readonly TimeSpan DefaultValidity = TimeSpan.FromMinutes(10);

    public OtpService(IdentityContext context, ILogger<OtpService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateOtpAsync(
        string userId, 
        OtpPurpose purpose, 
        TimeSpan? validity = null, 
        string? reference = null, 
        CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        // Invalidate existing OTPs for this user and purpose
        await InvalidateOtpsAsync(userId, purpose, cancellationToken);

        // Generate new OTP
        var otpValidity = validity ?? DefaultValidity;
        var otp = OneTimePassword.Generate(userId, purpose, otpValidity, reference);

        // Save to database
        await _context.OneTimePasswords.AddAsync(otp, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Generated OTP for user {UserId} for purpose {Purpose}", userId, purpose);

        return otp.Code;
    }

    public async Task<bool> VerifyOtpAsync(
        string userId, 
        string code, 
        OtpPurpose purpose, 
        bool markAsUsed = true, 
        CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));
        
        if (string.IsNullOrEmpty(code))
            throw new ArgumentNullException(nameof(code));

        // Find the most recent valid OTP
        var otp = await GetValidOtpAsync(userId, purpose, cancellationToken);

        if (otp == null)
        {
            _logger.LogWarning("No valid OTP found for user {UserId} for purpose {Purpose}", userId, purpose);
            return false;
        }

        // Verify the code
        var isValid = otp.Verify(code);

        if (isValid && markAsUsed)
        {
            // Mark as used
            otp.MarkAsUsed();
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("OTP verified and marked as used for user {UserId} for purpose {Purpose}", userId, purpose);
        }
        else if (!isValid)
        {
            _logger.LogWarning("Invalid OTP code provided for user {UserId} for purpose {Purpose}", userId, purpose);
        }

        return isValid;
    }

    public async Task<OneTimePassword?> GetValidOtpAsync(
        string userId, 
        OtpPurpose purpose, 
        CancellationToken cancellationToken = default)
    {
        return await _context.OneTimePasswords
            .Where(o => o.UserId == userId && 
                        o.Purpose == purpose && 
                        !o.IsUsed && 
                        o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InvalidateOtpsAsync(
        string userId, 
        OtpPurpose purpose, 
        CancellationToken cancellationToken = default)
    {
        var otpsToInvalidate = await _context.OneTimePasswords
            .Where(o => o.UserId == userId && 
                        o.Purpose == purpose && 
                        !o.IsUsed && 
                        o.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var otp in otpsToInvalidate)
        {
            otp.MarkAsUsed();
        }

        if (otpsToInvalidate.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Invalidated {Count} OTPs for user {UserId} for purpose {Purpose}", 
                otpsToInvalidate.Count, userId, purpose);
        }
    }
} 