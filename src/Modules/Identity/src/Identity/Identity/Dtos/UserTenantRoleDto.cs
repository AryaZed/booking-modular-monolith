using Identity.Identity.Models;

namespace Identity.Identity.Dtos;

public class UserTenantRoleDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public TenantType TenantType { get; set; }
    public string TenantName { get; set; }
    public long RoleId { get; set; }
    public string RoleName { get; set; }
}

public class TenantContext
{
    public long TenantId { get; set; }
    public TenantType TenantType { get; set; }
    public string RoleName { get; set; }
} 