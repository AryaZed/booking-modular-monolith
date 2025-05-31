using BuildingBlocks.Domain.Event;
using Identity.Data;
using Identity.Identity.Events;
using Identity.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Services;

public class TenantService : ITenantService
{
    private readonly IdentityContext _context;
    private readonly IModuleService _moduleService;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<TenantService> _logger;

    public TenantService(
        IdentityContext context,
        IModuleService moduleService,
        IEventDispatcher eventDispatcher,
        ILogger<TenantService> logger)
    {
        _context = context;
        _moduleService = moduleService;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<Tenant> GetTenantByIdAsync(long tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.ParentTenant)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> GetChildTenantsAsync(long parentTenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Where(t => t.ParentTenantId == parentTenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant> ReassignBranchToBrandAsync(
        long branchTenantId, 
        long newBrandTenantId, 
        CancellationToken cancellationToken = default)
    {
        // Validate the reassignment
        var validationResult = await ValidateTenantReassignmentAsync(branchTenantId, newBrandTenantId, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }

        // Get the tenants
        var branch = await _context.Tenants
            .Include(t => t.ParentTenant)
            .FirstOrDefaultAsync(t => t.Id == branchTenantId, cancellationToken);
            
        var newBrand = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == newBrandTenantId, cancellationToken);

        // Store the old parent ID for the event
        var oldBrandId = branch.ParentTenantId;
        
        // Update the branch's parent
        branch.UpdateParent(newBrandId: newBrandTenantId);
        
        // Save changes
        await _context.SaveChangesAsync(cancellationToken);
        
        // Create and dispatch domain event
        var branchReassignedEvent = new BranchReassignedEvent(
            BranchId: branchTenantId,
            OldBrandId: oldBrandId.Value,
            NewBrandId: newBrandTenantId);
            
        await _eventDispatcher.DispatchAsync(branchReassignedEvent, cancellationToken);
        
        _logger.LogInformation(
            "Branch {BranchId} reassigned from Brand {OldBrandId} to Brand {NewBrandId}",
            branchTenantId, oldBrandId, newBrandTenantId);
        
        return branch;
    }

    public async Task<(bool IsValid, string ErrorMessage)> ValidateTenantReassignmentAsync(
        long tenantId, 
        long newParentTenantId, 
        CancellationToken cancellationToken = default)
    {
        // Get the tenant to reassign
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
            
        if (tenant == null)
        {
            return (false, $"Tenant with ID {tenantId} not found");
        }
        
        // Verify tenant is a branch
        if (tenant.Type != TenantType.Branch)
        {
            return (false, $"Only branch tenants can be reassigned. Tenant {tenantId} is of type {tenant.Type}");
        }
        
        // Get the new parent tenant
        var newParent = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == newParentTenantId, cancellationToken);
            
        if (newParent == null)
        {
            return (false, $"New parent tenant with ID {newParentTenantId} not found");
        }
        
        // Verify new parent is a brand
        if (newParent.Type != TenantType.Brand)
        {
            return (false, $"Branches can only be assigned to brand tenants. Tenant {newParentTenantId} is of type {newParent.Type}");
        }
        
        // Check if new parent is same as current parent
        if (tenant.ParentTenantId == newParentTenantId)
        {
            return (false, $"Branch {tenantId} is already assigned to brand {newParentTenantId}");
        }
        
        return (true, string.Empty);
    }
} 