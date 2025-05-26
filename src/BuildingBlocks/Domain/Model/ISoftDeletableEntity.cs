using System;

namespace BuildingBlocks.Domain.Model
{
    public interface ISoftDeletableEntity
    {
        bool IsDeleted { get; }
        DateTime? DeletedAt { get; }
        long? DeletedBy { get; }
    }
} 