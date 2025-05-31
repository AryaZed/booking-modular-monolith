namespace Identity.Infrastructure.Services;

public interface ICurrentTenantProvider
{
    long? TenantId { get; }
    long? UserId { get; }
    string TenantKey { get; }
    
    void SetTenant(long tenantId, string tenantKey);
    void SetUser(long userId);
    void Clear();
} 