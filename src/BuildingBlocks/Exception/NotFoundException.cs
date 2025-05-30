using System;

namespace BuildingBlocks.Exception;

/// <summary>
/// Exception thrown when a requested resource cannot be found
/// </summary>
public class NotFoundException : CustomException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
