using Identity.Identity.Models;

namespace Identity.Identity.Features.Users.AssignUserToTenant;

public class AssignUserToTenantCommand
{
    // These are set from route parameters
    public long UserId { get; set; }
    public long TenantId { get; set; }
    
    // These come from the request body
    public TenantType TenantType { get; set; }
    public long RoleId { get; set; }
}

public class AssignUserToTenantResponse
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public TenantType TenantType { get; set; }
    public long RoleId { get; set; }
} 