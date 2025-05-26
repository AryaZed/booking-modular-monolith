using System;

namespace BuildingBlocks.Domain.Model
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; }
        long? CreatedBy { get; }
        DateTime? LastModifiedAt { get; }
        long? LastModifiedBy { get; }
    }
} 