using System.Linq;
using BuildingBlocks.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Authorization;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds permission-based authorization to the application
    /// </summary>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        // Register the authorization handler
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        
        // Get all permissions from the PermissionsConstant class using reflection
        var permissionType = typeof(PermissionsConstant);
        var nestedTypes = permissionType.GetNestedTypes();
        
        var authorizationOptions = new AuthorizationOptions();
        
        foreach (var nestedType in nestedTypes)
        {
            // Skip HashSet fields which are collections of permissions
            var permissions = nestedType.GetFields()
                .Where(f => f.FieldType == typeof(string))
                .Select(f => f.GetValue(null) as string)
                .Where(p => p != null);
                
            foreach (var permission in permissions)
            {
                services.AddAuthorization(options =>
                {
                    options.AddPolicy(
                        $"{RequirePermissionAttribute.PolicyPrefix}{permission}",
                        policy => policy.Requirements.Add(new PermissionRequirement(permission)));
                });
            }
        }
        
        return services;
    }
    
    /// <summary>
    /// Extension method to check if a user has a specific permission
    /// </summary>
    public static bool HasPermission(this System.Security.Claims.ClaimsPrincipal user, string permission)
    {
        return user.HasClaim(c => c.Type == "permission" && c.Value == permission);
    }
} 