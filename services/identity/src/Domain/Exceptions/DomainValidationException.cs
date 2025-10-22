namespace Vendo.Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when domain validation fails
/// </summary>
public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }

    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
