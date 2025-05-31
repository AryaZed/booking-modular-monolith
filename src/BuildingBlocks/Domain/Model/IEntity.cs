namespace BuildingBlocks.Domain.Model;

/// <summary>
/// Base interface for all entities in the domain model
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the creation date of the entity
    /// </summary>
    DateTime? CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who created the entity
    /// </summary>
    long? CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the last modification date of the entity
    /// </summary>
    DateTime? LastModified { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who last modified the entity
    /// </summary>
    long? LastModifiedBy { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted (soft delete)
    /// </summary>
    bool IsDeleted { get; set; }
}
