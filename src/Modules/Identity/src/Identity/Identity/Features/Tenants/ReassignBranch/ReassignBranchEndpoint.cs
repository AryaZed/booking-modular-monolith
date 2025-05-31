using System;
using System.Threading;
using System.Threading.Tasks;
using Identity.Identity.Dtos;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Constants;

namespace Identity.Identity.Features.Tenants.ReassignBranch;

[Route("api/identity/tenants")]
[ApiController]
[Authorize(Policy = IdentityConstant.Authorization.TenantManagementPolicy)]
public class ReassignBranchEndpoint : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<ReassignBranchEndpoint> _logger;

    public ReassignBranchEndpoint(
        ITenantService tenantService,
        ILogger<ReassignBranchEndpoint> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    [HttpPost("branches/{branchId}/reassign")]
    [ProducesResponseType(typeof(ReassignBranchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReassignBranchResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReassignBranchResponse>> ReassignBranch(
        [FromRoute] long branchId,
        [FromBody] ReassignBranchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate the reassignment
            var validationResult = await _tenantService.ValidateTenantReassignmentAsync(
                branchId, 
                request.NewBrandId, 
                cancellationToken);
                
            if (!validationResult.IsValid)
            {
                return BadRequest(new ReassignBranchResponse
                {
                    Success = false,
                    Message = validationResult.ErrorMessage
                });
            }

            // Perform the reassignment
            var branch = await _tenantService.ReassignBranchToBrandAsync(
                branchId, 
                request.NewBrandId, 
                cancellationToken);

            // Return success response
            return Ok(new ReassignBranchResponse
            {
                Success = true,
                BranchId = branch.Id,
                BranchName = branch.Name,
                NewBrandId = request.NewBrandId,
                Message = $"Branch '{branch.Name}' successfully reassigned to new brand"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reassigning branch {BranchId} to brand {BrandId}", 
                branchId, request.NewBrandId);
                
            return StatusCode(StatusCodes.Status500InternalServerError, new ReassignBranchResponse
            {
                Success = false,
                Message = "An error occurred while reassigning the branch"
            });
        }
    }
}

public class ReassignBranchRequest
{
    /// <summary>
    /// The ID of the new parent brand to assign the branch to
    /// </summary>
    public long NewBrandId { get; set; }
}

public class ReassignBranchResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public long BranchId { get; set; }
    public string BranchName { get; set; }
    public long NewBrandId { get; set; }
} 