namespace BuildingBlocks.Domain.Guard;

/// <summary>
/// Provides guard methods for defensive programming
/// </summary>
public static class Guard
{
    /// <summary>
    /// Guards against null values
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="parameterName">The name of the parameter being checked</param>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <returns>The value if not null</returns>
    /// <exception cref="ArgumentNullException">Thrown when the value is null</exception>
    public static T AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
            
        return value;
    }
    
    /// <summary>
    /// Guards against null or empty strings
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <param name="parameterName">The name of the parameter being checked</param>
    /// <returns>The string if not null or empty</returns>
    /// <exception cref="ArgumentException">Thrown when the string is null or empty</exception>
    public static string AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", parameterName);
            
        return value;
    }
    
    /// <summary>
    /// Guards against null or whitespace strings
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <param name="parameterName">The name of the parameter being checked</param>
    /// <returns>The string if not null or whitespace</returns>
    /// <exception cref="ArgumentException">Thrown when the string is null or whitespace</exception>
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace", parameterName);
            
        return value;
    }
    
    /// <summary>
    /// Guards against values that are outside of a specified range
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="parameterName">The name of the parameter being checked</param>
    /// <param name="min">The minimum allowed value</param>
    /// <param name="max">The maximum allowed value</param>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <returns>The value if within the range</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside the range</exception>
    public static T AgainstOutOfRange<T>(T value, string parameterName, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {min} and {max}");
            
        return value;
    }
    
    /// <summary>
    /// Guards against values that are less than a specified minimum
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="parameterName">The name of the parameter being checked</param>
    /// <param name="min">The minimum allowed value</param>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <returns>The value if greater than or equal to the minimum</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than the minimum</exception>
    public static T AgainstLessThan<T>(T value, string parameterName, T min) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be at least {min}");
            
        return value;
    }
    
    /// <summary>
    /// Guards against values that are greater than a specified maximum
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="parameterName">The name of the parameter being checked</param>
    /// <param name="max">The maximum allowed value</param>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <returns>The value if less than or equal to the maximum</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is greater than the maximum</exception>
    public static T AgainstGreaterThan<T>(T value, string parameterName, T max) where T : IComparable<T>
    {
        if (value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be at most {max}");
            
        return value;
    }
    
    /// <summary>
    /// Guards against values that don't satisfy a predicate
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="parameterName">The name of the parameter being checked</param>
    /// <param name="predicate">The predicate to satisfy</param>
    /// <param name="message">The exception message if the predicate is not satisfied</param>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <returns>The value if it satisfies the predicate</returns>
    /// <exception cref="ArgumentException">Thrown when the value doesn't satisfy the predicate</exception>
    public static T Against<T>(T value, string parameterName, Func<T, bool> predicate, string message)
    {
        if (!predicate(value))
            throw new ArgumentException(message, parameterName);
            
        return value;
    }
} 