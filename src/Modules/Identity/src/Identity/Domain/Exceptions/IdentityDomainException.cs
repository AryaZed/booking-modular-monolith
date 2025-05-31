namespace Identity.Domain.Exceptions;

public abstract class IdentityDomainException : Exception
{
    protected IdentityDomainException(string message) : base(message)
    {
    }

    protected IdentityDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
} 