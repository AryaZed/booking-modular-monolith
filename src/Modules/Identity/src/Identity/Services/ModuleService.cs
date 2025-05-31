using Identity.Data;
using Identity.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Services;

public class ModuleService : IModuleService
{
    private readonly IdentityContext _context;
    private readonly ILogger<ModuleService> _logger;

    public ModuleService(IdentityContext context, ILogger<ModuleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Module>> GetAllModulesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Modules.ToListAsync(cancellationToken);
    }

    public async Task<Module> GetModuleByIdAsync(long moduleId, CancellationToken cancellationToken = default)
    {
        return await _context.Modules.FindAsync(new object[] { moduleId }, cancellationToken);
    }

    public async Task<Module> GetModuleByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        code = code.ToLowerInvariant();
        return await _context.Modules
            .FirstOrDefaultAsync(m => m.Code == code, cancellationToken);
    }

    public async Task<Module> CreateModuleAsync(
        string name, 
        string code, 
        string description, 
        CancellationToken cancellationToken = default)
    {
        // Check if module with same code already exists
        var existingModule = await GetModuleByCodeAsync(code, cancellationToken);
        if (existingModule != null)
        {
            throw new InvalidOperationException($"Module with code '{code}' already exists");
        }

        var module = Module.Create(name, code, description);
        
        await _context.Modules.AddAsync(module, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created module {ModuleId}: {ModuleName} ({ModuleCode})", module.Id, module.Name, module.Code);
        
        return module;
    }

    public async Task<Module> UpdateModuleAsync(
        long moduleId, 
        string name, 
        string description, 
        bool isActive, 
        CancellationToken cancellationToken = default)
    {
        var module = await GetModuleByIdAsync(moduleId, cancellationToken);
        if (module == null)
        {
            throw new InvalidOperationException($"Module with ID '{moduleId}' not found");
        }

        module.Update(name, description, isActive);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Updated module {ModuleId}: {ModuleName}", module.Id, module.Name);
        
        return module;
    }

    public async Task<IEnumerable<TenantModule>> GetTenantModulesAsync(long tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TenantModules
            .Include(tm => tm.Module)
            .Where(tm => tm.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasModuleAccessAsync(long tenantId, string moduleCode, CancellationToken cancellationToken = default)
    {
        // Ensure code is lowercase for consistent lookup
        moduleCode = moduleCode.ToLowerInvariant();
        
        // Get the module first
        var module = await GetModuleByCodeAsync(moduleCode, cancellationToken);
        if (module == null)
        {
            _logger.LogWarning("Module access check failed: Module with code '{ModuleCode}' not found", moduleCode);
            return false;
        }
        
        // Check if tenant has active subscription to this module
        var tenantModule = await _context.TenantModules
            .FirstOrDefaultAsync(tm => 
                tm.TenantId == tenantId && 
                tm.ModuleId == module.Id && 
                tm.IsActive, 
                cancellationToken);
                
        if (tenantModule == null)
        {
            return false;
        }
        
        // Check if subscription is expired
        if (tenantModule.IsExpired())
        {
            _logger.LogInformation("Tenant {TenantId} subscription to module {ModuleCode} is expired", tenantId, moduleCode);
            return false;
        }
        
        return true;
    }

    public async Task<IEnumerable<string>> GetTenantModuleCodesAsync(long tenantId, CancellationToken cancellationToken = default)
    {
        var accessibleModules = await _context.TenantModules
            .Include(tm => tm.Module)
            .Where(tm => 
                tm.TenantId == tenantId && 
                tm.IsActive &&
                (!tm.ExpiresAt.HasValue || tm.ExpiresAt > DateTime.UtcNow))
            .Select(tm => tm.Module.Code)
            .ToListAsync(cancellationToken);
            
        return accessibleModules;
    }

    public async Task<TenantModule> SubscribeTenantToModuleAsync(
        long tenantId, 
        string moduleCode, 
        DateTime? expiresAt = null, 
        CancellationToken cancellationToken = default)
    {
        // Ensure tenant exists
        var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID '{tenantId}' not found");
        }
        
        // Ensure module exists
        var module = await GetModuleByCodeAsync(moduleCode, cancellationToken);
        if (module == null)
        {
            throw new InvalidOperationException($"Module with code '{moduleCode}' not found");
        }
        
        // Check if subscription already exists
        var existingSubscription = await _context.TenantModules
            .FirstOrDefaultAsync(tm => 
                tm.TenantId == tenantId && 
                tm.ModuleId == module.Id, 
                cancellationToken);
                
        if (existingSubscription != null)
        {
            // If subscription exists but is deactivated, reactivate it
            if (!existingSubscription.IsActive)
            {
                existingSubscription.Activate();
                existingSubscription.Renew(expiresAt);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Reactivated subscription for tenant {TenantId} to module {ModuleCode}", tenantId, moduleCode);
                
                return existingSubscription;
            }
            
            // Update expiry date if needed
            if (expiresAt.HasValue && 
                (!existingSubscription.ExpiresAt.HasValue || existingSubscription.ExpiresAt.Value != expiresAt.Value))
            {
                existingSubscription.Renew(expiresAt);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Updated expiry date for tenant {TenantId} subscription to module {ModuleCode}", tenantId, moduleCode);
                
                return existingSubscription;
            }
            
            return existingSubscription;
        }
        
        // Create new subscription
        var tenantModule = TenantModule.Create(tenantId, module.Id, expiresAt);
        
        await _context.TenantModules.AddAsync(tenantModule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Subscribed tenant {TenantId} to module {ModuleCode}", tenantId, moduleCode);
        
        return tenantModule;
    }

    public async Task<TenantModule> RenewTenantModuleAsync(
        long tenantId, 
        string moduleCode, 
        DateTime? newExpiryDate = null, 
        CancellationToken cancellationToken = default)
    {
        // Get module
        var module = await GetModuleByCodeAsync(moduleCode, cancellationToken);
        if (module == null)
        {
            throw new InvalidOperationException($"Module with code '{moduleCode}' not found");
        }
        
        // Get tenant module subscription
        var tenantModule = await _context.TenantModules
            .FirstOrDefaultAsync(tm => 
                tm.TenantId == tenantId && 
                tm.ModuleId == module.Id, 
                cancellationToken);
                
        if (tenantModule == null)
        {
            // Create new subscription if it doesn't exist
            return await SubscribeTenantToModuleAsync(tenantId, moduleCode, newExpiryDate, cancellationToken);
        }
        
        // Renew existing subscription
        tenantModule.Renew(newExpiryDate);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Renewed tenant {TenantId} subscription to module {ModuleCode}", tenantId, moduleCode);
        
        return tenantModule;
    }

    public async Task DeactivateTenantModuleAsync(
        long tenantId, 
        string moduleCode, 
        CancellationToken cancellationToken = default)
    {
        // Get module
        var module = await GetModuleByCodeAsync(moduleCode, cancellationToken);
        if (module == null)
        {
            throw new InvalidOperationException($"Module with code '{moduleCode}' not found");
        }
        
        // Get tenant module subscription
        var tenantModule = await _context.TenantModules
            .FirstOrDefaultAsync(tm => 
                tm.TenantId == tenantId && 
                tm.ModuleId == module.Id, 
                cancellationToken);
                
        if (tenantModule == null)
        {
            _logger.LogWarning("Cannot deactivate: Tenant {TenantId} is not subscribed to module {ModuleCode}", tenantId, moduleCode);
            return;
        }
        
        // Deactivate subscription
        tenantModule.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Deactivated tenant {TenantId} subscription to module {ModuleCode}", tenantId, moduleCode);
    }

    public async Task<TenantModule> ActivateTenantModuleAsync(
        long tenantId, 
        string moduleCode, 
        CancellationToken cancellationToken = default)
    {
        // Get module
        var module = await GetModuleByCodeAsync(moduleCode, cancellationToken);
        if (module == null)
        {
            throw new InvalidOperationException($"Module with code '{moduleCode}' not found");
        }
        
        // Get tenant module subscription
        var tenantModule = await _context.TenantModules
            .FirstOrDefaultAsync(tm => 
                tm.TenantId == tenantId && 
                tm.ModuleId == module.Id, 
                cancellationToken);
                
        if (tenantModule == null)
        {
            // Create new subscription if it doesn't exist
            return await SubscribeTenantToModuleAsync(tenantId, moduleCode, null, cancellationToken);
        }
        
        // Activate existing subscription
        tenantModule.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Activated tenant {TenantId} subscription to module {ModuleCode}", tenantId, moduleCode);
        
        return tenantModule;
    }
} 