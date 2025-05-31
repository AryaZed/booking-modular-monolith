using System.Reflection;

namespace BuildingBlocks.Domain.Model;

/// <summary>
/// Base class for creating strongly-typed enumerations in the domain model
/// </summary>
/// <typeparam name="TEnum">The type of the enumeration</typeparam>
public abstract class Enumeration<TEnum> : ValueObject, IComparable
    where TEnum : Enumeration<TEnum>
{
    private static readonly Dictionary<int, TEnum> Enumerations = CreateEnumerations();

    /// <summary>
    /// Gets the name of the enumeration
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the identifier of the enumeration
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Enumeration{TEnum}"/> class
    /// </summary>
    /// <param name="id">The identifier of the enumeration</param>
    /// <param name="name">The name of the enumeration</param>
    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Creates a dictionary of enumerations
    /// </summary>
    /// <returns>Dictionary of enumerations by their ID</returns>
    private static Dictionary<int, TEnum> CreateEnumerations()
    {
        var enumerationType = typeof(TEnum);

        var fieldsForType = enumerationType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fieldInfo => enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo => (TEnum)fieldInfo.GetValue(default)!);

        return fieldsForType.ToDictionary(enumeration => enumeration.Id);
    }

    /// <summary>
    /// Gets all defined enumerations
    /// </summary>
    /// <returns>List of all defined enumerations</returns>
    public static IEnumerable<TEnum> GetAll() => Enumerations.Values;

    /// <summary>
    /// Attempts to get an enumeration by its ID
    /// </summary>
    /// <param name="id">The ID of the enumeration</param>
    /// <param name="enumeration">The found enumeration or default value if not found</param>
    /// <returns>True if the enumeration was found, false otherwise</returns>
    public static bool TryFromId(int id, out TEnum? enumeration)
    {
        return Enumerations.TryGetValue(id, out enumeration);
    }

    /// <summary>
    /// Gets an enumeration by its ID
    /// </summary>
    /// <param name="id">The ID of the enumeration</param>
    /// <returns>The found enumeration</returns>
    /// <exception cref="InvalidOperationException">Thrown when the enumeration is not found</exception>
    public static TEnum FromId(int id)
    {
        if (TryFromId(id, out var enumeration))
        {
            return enumeration!;
        }

        throw new InvalidOperationException($"'{id}' is not a valid ID in {typeof(TEnum)}");
    }

    /// <summary>
    /// Attempts to parse an enumeration from its name
    /// </summary>
    /// <param name="name">The name of the enumeration</param>
    /// <param name="ignoreCase">Whether to ignore case when comparing names</param>
    /// <returns>The found enumeration or null if not found</returns>
    public static TEnum? FromName(string name, bool ignoreCase = false)
    {
        var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        return GetAll().SingleOrDefault(e => string.Equals(e.Name, name, stringComparison));
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// Gets the components used for equality comparison
    /// </summary>
    /// <returns>Enumerable of components</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }

    /// <summary>
    /// Compares this enumeration to another object
    /// </summary>
    /// <param name="other">The object to compare with</param>
    /// <returns>A value indicating the relative order</returns>
    public int CompareTo(object? other) => Id.CompareTo(((Enumeration<TEnum>)other!).Id);
} 