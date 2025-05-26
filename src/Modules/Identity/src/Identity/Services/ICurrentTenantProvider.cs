using Identity.Identity.Models;

namespace Identity.Services;

public interface ICurrentTenantProvider
{
    long? TenantId { get; }
    TenantType? TenantType { get; }
} 