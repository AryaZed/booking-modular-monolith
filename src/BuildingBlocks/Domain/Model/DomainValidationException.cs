using System;

namespace BuildingBlocks.Domain.Model
{
    public class DomainValidationException : System.Exception
    {
        public DomainValidationException(string message) : base(message)
        {
        }
        
        public DomainValidationException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
} 