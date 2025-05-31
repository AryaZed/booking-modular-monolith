using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Domain.Event;
using DotNetCore.CAP;
using Identity.Data;
using Identity.Identity.Events;
using Identity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Identity.EventHandlers;

public class BranchReassignedEventHandler : IDomainEventHandler<BranchReassignedEvent>
{
    private readonly IdentityContext _context;
    private readonly ICapPublisher _capPublisher;
    private readonly IModuleService _moduleService;
    private readonly ILogger<BranchReassignedEventHandler> _logger;

    public BranchReassignedEventHandler(
        IdentityContext context,
        ICapPublisher capPublisher,
        IModuleService moduleService,
        ILogger<BranchReassignedEventHandler> logger)
    {
        _context = context;
        _capPublisher = capPublisher;
        _moduleService = moduleService;
        _logger = logger;
    }

    public async Task Handle(BranchReassignedEvent domainEvent, CancellationToken cancellationToken)
    {
        // Load tenant details for the integration event
        var branch = await _context.Tenants.FindAsync(new object[] { domainEvent.BranchId }, cancellationToken);
        var oldBrand = await _context.Tenants.FindAsync(new object[] { domainEvent.OldBrandId }, cancellationToken);
        var newBrand = await _context.Tenants.FindAsync(new object[] { domainEvent.NewBrandId }, cancellationToken);

        if (branch == null || oldBrand == null || newBrand == null)
        {
            _logger.LogWarning("Failed to find all tenants for branch reassignment: Branch={BranchId}, OldBrand={OldBrandId}, NewBrand={NewBrandId}",
                domainEvent.BranchId, domainEvent.OldBrandId, domainEvent.NewBrandId);
            return;
        }

        // Handle module access inheritance
        await HandleModuleAccessInheritanceAsync(domainEvent.BranchId, domainEvent.NewBrandId, cancellationToken);

        // Create and publish the integration event
        var integrationEvent = new BranchReassignedIntegrationEvent(
            branchId: domainEvent.BranchId,
            oldBrandId: domainEvent.OldBrandId,
            newBrandId: domainEvent.NewBrandId,
            branchName: branch.Name,
            oldBrandName: oldBrand.Name,
            newBrandName: newBrand.Name);

        // Use CAP to publish the integration event to the message broker
        await _capPublisher.PublishAsync("identity.branch.reassigned", integrationEvent, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published branch reassignment integration event: Branch={BranchId} ({BranchName}) moved from Brand={OldBrandId} ({OldBrandName}) to Brand={NewBrandId} ({NewBrandName})",
            branch.Id, branch.Name, oldBrand.Id, oldBrand.Name, newBrand.Id, newBrand.Name);
    }

    /// <summary>
    /// Handles updating module access for the branch based on the new parent brand's subscriptions
    /// </summary>
    private async Task HandleModuleAccessInheritanceAsync(long branchId, long newBrandId, CancellationToken cancellationToken)
    {
        // Get all modules the new brand has access to
        var brandModules = await _moduleService.GetTenantModulesAsync(newBrandId, cancellationToken);
        
        // For each module the brand has access to, ensure the branch has access
        foreach (var brandModule in brandModules)
        {
            if (brandModule.HasAccess())
            {
                // Check if branch already has this module
                var branchModule = await _context.TenantModules
                    .FirstOrDefaultAsync(tm => 
                        tm.TenantId == branchId && 
                        tm.ModuleId == brandModule.ModuleId, 
                        cancellationToken);
                
                if (branchModule == null)
                {
                    // Subscribe branch to this module with same expiry as brand
                    var newBranchModule = TenantModule.Create(
                        branchId, 
                        brandModule.ModuleId, 
                        brandModule.ExpiresAt);
                        
                    await _context.TenantModules.AddAsync(newBranchModule, cancellationToken);
                    
                    _logger.LogInformation(
                        "Added module access for reassigned branch: Branch={BranchId}, Module={ModuleId}",
                        branchId, brandModule.ModuleId);
                }
                else if (!branchModule.IsActive)
                {
                    // Activate existing module subscription if inactive
                    branchModule.Activate();
                    
                    _logger.LogInformation(
                        "Activated module access for reassigned branch: Branch={BranchId}, Module={ModuleId}",
                        branchId, brandModule.ModuleId);
                }
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
} 