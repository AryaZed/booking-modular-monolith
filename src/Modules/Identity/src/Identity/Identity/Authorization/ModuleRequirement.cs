using Microsoft.AspNetCore.Authorization;

namespace Identity.Identity.Authorization;

/// <summary>
/// Authorization requirement that requires access to a specific module
/// </summary>
public class ModuleRequirement : IAuthorizationRequirement
{
    public string ModuleCode { get; }
    
    public ModuleRequirement(string moduleCode)
    {
        ModuleCode = moduleCode.ToLowerInvariant();
    }
} 