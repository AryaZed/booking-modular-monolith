using System.Linq.Expressions;
using Identity.Application.Interfaces;
using Identity.Domain.Aggregates.Tenant;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly IdentityContext _context;

    public TenantRepository(IdentityContext context)
    {
        _context = context;
    }

    public async Task<Tenant> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.ParentTenant)
            .Include(t => t.ChildTenants)
            .Include(t => t.Modules)
                .ThenInclude(tm => tm.Module)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.ParentTenant)
            .Include(t => t.ChildTenants)
            .Include(t => t.Modules)
                .ThenInclude(tm => tm.Module)
            .FirstOrDefaultAsync(t => t.Key == key.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.ParentTenant)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetChildTenantsAsync(long parentTenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Where(t => t.ParentTenantId == parentTenantId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> FindAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Where(predicate)
            .Include(t => t.ParentTenant)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        // Implement as soft delete
        tenant.SoftDelete();
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants.AnyAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<Module>> GetModulesAsync(long tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.TenantModules
            .Where(tm => tm.TenantId == tenantId)
            .Select(tm => tm.Module)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantModule> AddModuleToTenantAsync(long tenantId, long moduleId, bool isEnabled = true, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken);
        var module = await _context.Modules.FindAsync(new object[] { moduleId }, cancellationToken);

        if (tenant == null || module == null)
        {
            throw new InvalidOperationException("Tenant or module not found");
        }

        var tenantModule = TenantModule.Create(tenant, module, isEnabled);
        await _context.TenantModules.AddAsync(tenantModule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return tenantModule;
    }

    public async Task RemoveModuleFromTenantAsync(long tenantId, long moduleId, CancellationToken cancellationToken = default)
    {
        var tenantModule = await _context.TenantModules
            .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.ModuleId == moduleId, cancellationToken);

        if (tenantModule != null)
        {
            _context.TenantModules.Remove(tenantModule);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsModuleEnabledForTenantAsync(long tenantId, string moduleKey, CancellationToken cancellationToken = default)
    {
        return await _context.TenantModules
            .Include(tm => tm.Module)
            .AnyAsync(tm => tm.TenantId == tenantId && 
                           tm.Module.Key == moduleKey.ToLowerInvariant() && 
                           tm.IsEnabled && 
                           tm.Module.IsEnabled, 
                cancellationToken);
    }
} 