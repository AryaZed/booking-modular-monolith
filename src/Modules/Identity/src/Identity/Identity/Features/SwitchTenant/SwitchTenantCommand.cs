using Identity.Identity.Models;

namespace Identity.Identity.Features.SwitchTenant;

public class SwitchTenantCommand
{
    public long TenantId { get; set; }
    public TenantType TenantType { get; set; }
} 