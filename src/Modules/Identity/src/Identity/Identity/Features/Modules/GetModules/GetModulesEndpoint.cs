using Identity.Identity.Authorization;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Identity.Features.Modules.GetModules;

[Route("api/identity/modules")]
[ApiController]
[Authorize]
public class GetModulesEndpoint : ControllerBase
{
    private readonly IModuleService _moduleService;
    private readonly ICurrentTenantProvider _tenantProvider;
    private readonly ILogger<GetModulesEndpoint> _logger;

    public GetModulesEndpoint(
        IModuleService moduleService,
        ICurrentTenantProvider tenantProvider,
        ILogger<GetModulesEndpoint> logger)
    {
        _moduleService = moduleService;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GetModulesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetModulesResponse>> GetModules(CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = _tenantProvider.TenantId;
            if (!tenantId.HasValue)
            {
                return Ok(new GetModulesResponse
                {
                    Success = true,
                    Modules = new List<ModuleDto>(),
                    Message = "No tenant context available"
                });
            }

            var modules = await _moduleService.GetTenantModulesAsync(tenantId.Value, cancellationToken);
            
            var response = new GetModulesResponse
            {
                Success = true,
                Modules = modules.Select(tm => new ModuleDto
                {
                    Id = tm.Module.Id,
                    Code = tm.Module.Code,
                    Name = tm.Module.Name,
                    Description = tm.Module.Description,
                    IsActive = tm.IsActive && tm.Module.IsActive,
                    SubscribedAt = tm.SubscribedAt,
                    ExpiresAt = tm.ExpiresAt,
                    HasAccess = tm.HasAccess()
                }).ToList()
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant modules");
            return StatusCode(StatusCodes.Status500InternalServerError, new GetModulesResponse
            {
                Success = false,
                Message = "Error retrieving modules"
            });
        }
    }
    
    [HttpGet("test-booking")]
    [Authorize(Policy = "Module:booking")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult TestBookingAccess()
    {
        return Ok("You have access to the booking module");
    }
    
    [HttpGet("test-analytics")]
    [Authorize(Policy = "Module:analytics")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult TestAnalyticsAccess()
    {
        return Ok("You have access to the analytics module");
    }
}

public class GetModulesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<ModuleDto> Modules { get; set; } = new List<ModuleDto>();
}

public class ModuleDto
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime SubscribedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool HasAccess { get; set; }
} 