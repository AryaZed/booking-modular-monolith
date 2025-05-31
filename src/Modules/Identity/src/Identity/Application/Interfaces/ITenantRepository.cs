using System.Linq.Expressions;
using Identity.Domain.Aggregates.Tenant;

namespace Identity.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Tenant> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> GetChildTenantsAsync(long parentTenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tenant>> FindAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken = default);
    Task<Tenant> AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task DeleteAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Module>> GetModulesAsync(long tenantId, CancellationToken cancellationToken = default);
    Task<TenantModule> AddModuleToTenantAsync(long tenantId, long moduleId, bool isEnabled = true, CancellationToken cancellationToken = default);
    Task RemoveModuleFromTenantAsync(long tenantId, long moduleId, CancellationToken cancellationToken = default);
    Task<bool> IsModuleEnabledForTenantAsync(long tenantId, string moduleKey, CancellationToken cancellationToken = default);
} 