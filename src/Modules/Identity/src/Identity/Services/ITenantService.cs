using Identity.Identity.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Services;

public interface ITenantService
{
    /// <summary>
    /// Gets a tenant by ID
    /// </summary>
    Task<Tenant> GetTenantByIdAsync(long tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all child tenants of a parent tenant
    /// </summary>
    Task<IEnumerable<Tenant>> GetChildTenantsAsync(long parentTenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reassigns a branch tenant to a new parent brand tenant
    /// </summary>
    Task<Tenant> ReassignBranchToBrandAsync(long branchTenantId, long newBrandTenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates if a tenant can be reassigned to a new parent
    /// </summary>
    Task<(bool IsValid, string ErrorMessage)> ValidateTenantReassignmentAsync(
        long tenantId, 
        long newParentTenantId, 
        CancellationToken cancellationToken = default);
} 