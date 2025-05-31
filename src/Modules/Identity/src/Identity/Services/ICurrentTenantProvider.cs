using Identity.Identity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Services;

public interface ICurrentTenantProvider
{
    long? TenantId { get; }
    TenantType? TenantType { get; }
    
    /// <summary>
    /// Checks if the current tenant has access to the specified module
    /// </summary>
    Task<bool> HasModuleAccessAsync(string moduleCode);
    
    /// <summary>
    /// Gets all module codes that the current tenant has access to
    /// </summary>
    Task<IReadOnlyList<string>> GetAccessibleModuleCodesAsync();
} 