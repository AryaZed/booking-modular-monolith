using System.Collections.Generic;

namespace BuildingBlocks.Security;

/// <summary>
/// Interface for accessing the current authenticated user's information
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    long? Id { get; }
    
    /// <summary>
    /// Gets the current user's username
    /// </summary>
    string Username { get; }
    
    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string Email { get; }
    
    /// <summary>
    /// Gets the current user's first name
    /// </summary>
    string FirstName { get; }
    
    /// <summary>
    /// Gets the current user's last name
    /// </summary>
    string LastName { get; }
    
    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    IReadOnlyList<string> Roles { get; }
    
    /// <summary>
    /// Gets the current user's permissions
    /// </summary>
    IReadOnlyList<string> Permissions { get; }
    
    /// <summary>
    /// Gets the current tenant ID if available
    /// </summary>
    long? TenantId { get; }
    
    /// <summary>
    /// Gets whether the user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Checks if the current user has a specific permission
    /// </summary>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the user has the permission, false otherwise</returns>
    bool HasPermission(string permission);
    
    /// <summary>
    /// Checks if the current user is in a specific role
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user is in the role, false otherwise</returns>
    bool IsInRole(string role);
} 