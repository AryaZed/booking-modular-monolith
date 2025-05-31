using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Identity.Services;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.Features.Tenants.Consumers;

/// <summary>
/// Consumes branch reassignment events from the Profiles module
/// to sync changes to the Identity module
/// </summary>
public class ProfilesBranchReassignedConsumer : ICapSubscribe
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<ProfilesBranchReassignedConsumer> _logger;

    public ProfilesBranchReassignedConsumer(
        ITenantService tenantService,
        ILogger<ProfilesBranchReassignedConsumer> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    [CapSubscribe("profiles.branch.reassigned")]
    public async Task HandleBranchReassignedEvent(ProfilesBranchReassignedEvent @event)
    {
        try
        {
            _logger.LogInformation(
                "Received branch reassignment event from Profiles module: Branch={BranchId}, NewBrand={NewBrandId}",
                @event.BranchId, @event.NewBrandId);

            // Validate the reassignment
            var validationResult = await _tenantService.ValidateTenantReassignmentAsync(
                @event.BranchId, 
                @event.NewBrandId);
                
            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "Invalid branch reassignment from Profiles module: {ErrorMessage}",
                    validationResult.ErrorMessage);
                return;
            }

            // Perform the reassignment
            var branch = await _tenantService.ReassignBranchToBrandAsync(
                @event.BranchId, 
                @event.NewBrandId);
                
            _logger.LogInformation(
                "Successfully synchronized branch reassignment from Profiles module: Branch={BranchId} ({BranchName}) to Brand={NewBrandId}",
                branch.Id, branch.Name, @event.NewBrandId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error processing branch reassignment event from Profiles module: Branch={BranchId}, NewBrand={NewBrandId}",
                @event.BranchId, @event.NewBrandId);
                
            // Consider retrying or alerting here
            throw;
        }
    }
}

/// <summary>
/// Event data structure for branch reassignment events from the Profiles module
/// </summary>
public class ProfilesBranchReassignedEvent
{
    public long BranchId { get; set; }
    public long OldBrandId { get; set; }
    public long NewBrandId { get; set; }
    public DateTime ReassignedAt { get; set; }
} 