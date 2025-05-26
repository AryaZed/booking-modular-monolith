using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BuildingBlocks.Constants;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Identity.Identity.Features.Permissions.GetAllPermissions;

[Route(BaseApiPath + "/identity/permissions")]
public class GetAllPermissionsEndpoint : BaseController
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(Summary = "Get all permissions", Description = "Returns all available permissions defined in the system")]
    public ActionResult GetAllPermissions()
    {
        var permissions = new Dictionary<string, List<PermissionDto>>();
        
        // Use reflection to get all permission categories and values
        var permissionsType = typeof(PermissionsConstant);
        var nestedTypes = permissionsType.GetNestedTypes();
        
        foreach (var nestedType in nestedTypes)
        {
            var categoryName = nestedType.Name;
            
            // Skip the predefined permission sets (they're not categories)
            if (categoryName == nameof(PermissionsConstant.BrandLevelPermissions) || 
                categoryName == nameof(PermissionsConstant.BranchLevelPermissions))
                continue;
                
            var categoryPermissions = new List<PermissionDto>();
            
            // Get all string constants in this category
            var permissionFields = nestedType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == typeof(string));
                
            foreach (var field in permissionFields)
            {
                var value = field.GetValue(null) as string;
                if (value != null)
                {
                    categoryPermissions.Add(new PermissionDto
                    {
                        Name = field.Name,
                        Value = value
                    });
                }
            }
            
            permissions[categoryName] = categoryPermissions;
        }
        
        return Ok(new GetAllPermissionsResponse
        {
            Categories = permissions.Select(kvp => new PermissionCategoryDto
            {
                Name = kvp.Key,
                Permissions = kvp.Value
            }).ToList()
        });
    }
} 