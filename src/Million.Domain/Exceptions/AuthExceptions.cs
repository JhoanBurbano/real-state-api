namespace Million.Domain.Exceptions;

public class AuthenticationException : DomainException
{
    public AuthenticationException(string message) : base(message, "AUTHENTICATION_ERROR", 401) { }
}

public class AuthorizationException : DomainException
{
    public AuthorizationException(string message) : base(message, "AUTHORIZATION_ERROR", 403) { }
}

public class InvalidCredentialsException : AuthenticationException
{
    public InvalidCredentialsException() : base("Invalid email or password") { }
}

public class AccountLockedException : AuthenticationException
{
    public AccountLockedException() : base("Account is temporarily locked due to multiple failed login attempts") { }
}

public class AccountInactiveException : AuthenticationException
{
    public AccountInactiveException() : base("Account is inactive") { }
}

public class InvalidTokenException : AuthenticationException
{
    public InvalidTokenException() : base("Invalid or expired token") { }
}

public class TokenExpiredException : AuthenticationException
{
    public TokenExpiredException() : base("Token has expired") { }
}

public class RefreshTokenRevokedException : AuthenticationException
{
    public RefreshTokenRevokedException() : base("Refresh token has been revoked") { }
}

public class InsufficientPermissionsException : AuthorizationException
{
    public InsufficientPermissionsException(string action) : base($"Insufficient permissions to {action}") { }
}

public class OwnerNotFoundException : DomainException
{
    public OwnerNotFoundException(string id) : base($"Owner with ID '{id}' not found", "OWNER_NOT_FOUND", 404) { }
}

public class OwnerSessionNotFoundException : DomainException
{
    public OwnerSessionNotFoundException(string id) : base($"Owner session with ID '{id}' not found", "OWNER_SESSION_NOT_FOUND", 404) { }
}

public class DuplicateEmailException : DomainException
{
    public DuplicateEmailException(string email) : base($"Owner with email '{email}' already exists", "DUPLICATE_EMAIL", 409) { }
}

public class DuplicateCodeInternalException : DomainException
{
    public DuplicateCodeInternalException(string code) : base($"Property with internal code '{code}' already exists", "DUPLICATE_CODE_INTERNAL", 409) { }
}
