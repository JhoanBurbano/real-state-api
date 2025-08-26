namespace Million.Domain.Exceptions;

/// <summary>
/// Base exception for domain-related errors
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    protected DomainException(string message, string errorCode, int statusCode = 400)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    protected DomainException(string message, string errorCode, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Thrown when a property is not found
/// </summary>
public class PropertyNotFoundException : DomainException
{
    public string PropertyId { get; }

    public PropertyNotFoundException(string propertyId)
        : base($"Property with id '{propertyId}' not found", "PROPERTY_NOT_FOUND", 404)
    {
        PropertyId = propertyId;
    }
}

/// <summary>
/// Thrown when a property is already inactive
/// </summary>
public class PropertyAlreadyInactiveException : DomainException
{
    public string Code { get; }

    public PropertyAlreadyInactiveException(string propertyId)
        : base($"Property with id '{propertyId}' is already inactive", "PROPERTY_ALREADY_INACTIVE", 409)
    {
        Code = propertyId;
    }
}

/// <summary>
/// Thrown when a property is already active
/// </summary>
public class PropertyAlreadyActiveException : DomainException
{
    public string Code { get; }

    public PropertyAlreadyActiveException(string propertyId)
        : base($"Property with id '{propertyId}' is already active", "PROPERTY_ALREADY_ACTIVE", 409)
    {
        Code = propertyId;
    }
}

/// <summary>
/// Thrown when validation fails at domain level
/// </summary>
public class DomainValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    public DomainValidationException(IReadOnlyDictionary<string, string[]> validationErrors)
        : base("Domain validation failed", "DOMAIN_VALIDATION_FAILED", 422)
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base(message, "BUSINESS_RULE_VIOLATION", 422)
    {
        RuleName = ruleName;
    }
}
