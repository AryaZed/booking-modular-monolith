using System.Threading.Tasks;

namespace Booking.Services;

/// <summary>
/// Service for managing user profiles in the Booking module
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Creates a user profile in the Booking module based on a user created in Identity
    /// </summary>
    /// <param name="userId">User ID from Identity module</param>
    /// <param name="email">User email</param>
    /// <param name="fullName">User's full name</param>
    /// <param name="tenantId">Optional tenant ID the user belongs to</param>
    /// <param name="tenantType">Optional tenant type</param>
    Task<long> CreateUserProfileAsync(long userId, string email, string fullName, long? tenantId, string tenantType);
    
    /// <summary>
    /// Updates a user profile in the Booking module
    /// </summary>
    /// <param name="userId">User ID from Identity module</param>
    /// <param name="email">Updated email</param>
    /// <param name="fullName">Updated full name</param>
    /// <param name="isActive">User active status</param>
    Task UpdateUserProfileAsync(long userId, string email, string fullName, bool isActive);
    
    /// <summary>
    /// Marks a user profile as deleted in the Booking module
    /// </summary>
    /// <param name="userId">User ID from Identity module</param>
    Task DeleteUserProfileAsync(long userId);
} 