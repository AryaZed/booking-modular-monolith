namespace BuildingBlocks.Domain.Model;

/// <summary>
/// Base implementation of the IEntity interface
/// </summary>
public abstract class Entity : IEntity
{
    /// <inheritdoc/>
    public DateTime? CreatedAt { get; set; }
    
    /// <inheritdoc/>
    public long? CreatedBy { get; set; }
    
    /// <inheritdoc/>
    public DateTime? LastModified { get; set; }
    
    /// <inheritdoc/>
    public long? LastModifiedBy { get; set; }
    
    /// <inheritdoc/>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Checks if the current entity is equal to another entity
    /// </summary>
    /// <param name="obj">The object to compare with</param>
    /// <returns>True if the entities are equal, false otherwise</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return true;
    }

    /// <summary>
    /// Gets the hash code for the entity
    /// </summary>
    /// <returns>The hash code</returns>
    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }
}
