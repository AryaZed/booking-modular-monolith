using System.Collections.Generic;

namespace Identity.Identity.Features.Permissions.GetAllPermissions;

public class GetAllPermissionsResponse
{
    public List<PermissionCategoryDto> Categories { get; set; }
}

public class PermissionCategoryDto
{
    public string Name { get; set; }
    public List<PermissionDto> Permissions { get; set; }
}

public class PermissionDto
{
    public string Name { get; set; }
    public string Value { get; set; }
} 