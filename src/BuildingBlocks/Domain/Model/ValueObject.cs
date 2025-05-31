namespace BuildingBlocks.Domain.Model;

/// <summary>
/// Base class for value objects in the domain model
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// When overridden in a derived class, returns all components of a value object that should be used for equality
    /// </summary>
    /// <returns>An array of objects representing the value object's components</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object
    /// </summary>
    /// <param name="obj">The object to compare with the current value object</param>
    /// <returns>true if the specified object is equal to the current value object; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Serves as the default hash function
    /// </summary>
    /// <returns>A hash code for the current value object</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Determines whether two value objects are equal
    /// </summary>
    /// <param name="left">The first value object</param>
    /// <param name="right">The second value object</param>
    /// <returns>true if the value objects are equal; otherwise, false</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two value objects are not equal
    /// </summary>
    /// <param name="left">The first value object</param>
    /// <param name="right">The second value object</param>
    /// <returns>true if the value objects are not equal; otherwise, false</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
} 