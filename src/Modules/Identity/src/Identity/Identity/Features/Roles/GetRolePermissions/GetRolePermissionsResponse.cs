using System.Collections.Generic;

namespace Identity.Identity.Features.Roles.GetRolePermissions;

public class GetRolePermissionsResponse
{
    public IEnumerable<string> Permissions { get; set; }
} 