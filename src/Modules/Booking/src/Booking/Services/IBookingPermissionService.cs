using System.Threading.Tasks;

namespace Booking.Services;

/// <summary>
/// Service for managing permissions in the Booking module based on Identity module roles
/// </summary>
public interface IBookingPermissionService
{
    /// <summary>
    /// Syncs a user's permissions in the Booking module based on their roles in Identity module
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="tenantType">Tenant type</param>
    Task SyncUserPermissionsAsync(long userId, long roleId, long tenantId, string tenantType);
    
    /// <summary>
    /// Removes a user's permissions in the Booking module for a specific role
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="tenantId">Tenant ID</param>
    Task RemoveUserPermissionsAsync(long userId, long roleId, long tenantId);
} 