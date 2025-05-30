using System;

namespace BuildingBlocks.Domain;

/// <summary>
/// Exception thrown when a requested resource cannot be found
/// </summary>
public class NotFoundException : System.Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
    
    public NotFoundException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
}