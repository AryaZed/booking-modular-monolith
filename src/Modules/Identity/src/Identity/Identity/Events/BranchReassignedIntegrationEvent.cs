using System;

namespace Identity.Identity.Events;

/// <summary>
/// Integration event that is published when a branch is reassigned to a new brand
/// to notify other modules about the change
/// </summary>
public class BranchReassignedIntegrationEvent
{
    public long BranchId { get; }
    public long OldBrandId { get; }
    public long NewBrandId { get; }
    public string BranchName { get; }
    public string OldBrandName { get; }
    public string NewBrandName { get; }
    public DateTime ReassignedAt { get; }

    public BranchReassignedIntegrationEvent(
        long branchId,
        long oldBrandId,
        long newBrandId,
        string branchName,
        string oldBrandName,
        string newBrandName)
    {
        BranchId = branchId;
        OldBrandId = oldBrandId;
        NewBrandId = newBrandId;
        BranchName = branchName;
        OldBrandName = oldBrandName;
        NewBrandName = newBrandName;
        ReassignedAt = DateTime.UtcNow;
    }
} 