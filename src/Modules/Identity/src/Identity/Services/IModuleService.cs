using Identity.Identity.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Services;

public interface IModuleService
{
    /// <summary>
    /// Gets all available modules
    /// </summary>
    Task<IEnumerable<Module>> GetAllModulesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a module by ID
    /// </summary>
    Task<Module> GetModuleByIdAsync(long moduleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a module by code
    /// </summary>
    Task<Module> GetModuleByCodeAsync(string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new module
    /// </summary>
    Task<Module> CreateModuleAsync(string name, string code, string description, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing module
    /// </summary>
    Task<Module> UpdateModuleAsync(long moduleId, string name, string description, bool isActive, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all module subscriptions for a tenant
    /// </summary>
    Task<IEnumerable<TenantModule>> GetTenantModulesAsync(long tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a tenant has access to a specific module
    /// </summary>
    Task<bool> HasModuleAccessAsync(long tenantId, string moduleCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all accessible module codes for a tenant
    /// </summary>
    Task<IEnumerable<string>> GetTenantModuleCodesAsync(long tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Subscribes a tenant to a module
    /// </summary>
    Task<TenantModule> SubscribeTenantToModuleAsync(long tenantId, string moduleCode, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Renews a tenant's subscription to a module
    /// </summary>
    Task<TenantModule> RenewTenantModuleAsync(long tenantId, string moduleCode, DateTime? newExpiryDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deactivates a tenant's subscription to a module
    /// </summary>
    Task DeactivateTenantModuleAsync(long tenantId, string moduleCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Activates a tenant's subscription to a module
    /// </summary>
    Task<TenantModule> ActivateTenantModuleAsync(long tenantId, string moduleCode, CancellationToken cancellationToken = default);
} 