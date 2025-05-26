using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Authorization;

/// <summary>
/// Authorization attribute that checks if the user has the specified permission
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";
    
    public RequirePermissionAttribute(string permission) 
        : base(PolicyPrefix + permission)
    {
    }
} 