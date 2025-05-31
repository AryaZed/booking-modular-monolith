using BuildingBlocks.Domain.Event;

namespace Identity.Identity.Events;

/// <summary>
/// Domain event raised when a branch is reassigned to a new brand
/// </summary>
/// <param name="BranchId">The ID of the branch tenant being reassigned</param>
/// <param name="OldBrandId">The ID of the original parent brand tenant</param>
/// <param name="NewBrandId">The ID of the new parent brand tenant</param>
public record BranchReassignedEvent(long BranchId, long OldBrandId, long NewBrandId) : IDomainEvent; 