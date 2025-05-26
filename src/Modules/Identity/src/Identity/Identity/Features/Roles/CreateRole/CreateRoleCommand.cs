using System.Collections.Generic;

namespace Identity.Identity.Features.Roles.CreateRole;

public class CreateRoleCommand
{
    // This is set from the route parameter
    public long TenantId { get; set; }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}

public class CreateRoleResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
} 