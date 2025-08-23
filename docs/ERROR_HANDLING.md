# Million Properties API - Enhanced Error Handling Guide 🚨

## Overview

The Million Properties API implements a comprehensive, enterprise-grade error handling system that provides consistent, traceable, and actionable error responses following industry best practices and RFC standards.

## 🏗️ Architecture Overview

### **Error Handling Stack**
```
HTTP Request → CorrelationIdMiddleware → StructuredLoggingMiddleware → AdvancedRateLimiterMiddleware → ProblemDetailsMiddleware → Application Logic
                ↓
            Response ← Structured Logging ← Rate Limit Headers ← ProblemDetails ← Business Logic
```

### **Key Components**
1. **Domain Exceptions** - Business-specific error types
2. **ProblemDetails Middleware** - RFC 7807 compliant error responses
3. **Advanced Rate Limiting** - Intelligent request throttling
4. **Structured Logging** - Comprehensive request/response tracking
5. **Correlation IDs** - End-to-end request tracing

## 🚨 Domain Exceptions

### **Exception Hierarchy**
```csharp
Exception
└── DomainException (abstract)
    ├── PropertyNotFoundException (404)
    ├── PropertyAlreadyActiveException (409)
    ├── PropertyAlreadyInactiveException (409)
    ├── DomainValidationException (422)
    └── BusinessRuleViolationException (422)
```

### **Exception Types & HTTP Status Codes**

#### **404 Not Found**
```csharp
public class PropertyNotFoundException : DomainException
{
    public string PropertyId { get; }
    
    public PropertyNotFoundException(string propertyId) 
        : base($"Property with id '{propertyId}' not found", "PROPERTY_NOT_FOUND", 404)
    {
        PropertyId = propertyId;
    }
}
```

**Use Cases:**
- Property not found by ID
- Resource doesn't exist
- Invalid identifier provided

#### **409 Conflict**
```csharp
public class PropertyAlreadyActiveException : DomainException
{
    public PropertyAlreadyActiveException(string propertyId) 
        : base($"Property with id '{propertyId}' is already active", "PROPERTY_ALREADY_ACTIVE", 409)
}
```

**Use Cases:**
- Property already in desired state
- Duplicate resource creation
- Business rule conflicts

#### **422 Unprocessable Entity**
```csharp
public class DomainValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }
    
    public DomainValidationException(IReadOnlyDictionary<string, string[]> validationErrors) 
        : base("Domain validation failed", "DOMAIN_VALIDATION_FAILED", 422)
    {
        ValidationErrors = validationErrors;
    }
}
```

**Use Cases:**
- Business rule violations
- Domain-level validation failures
- Complex validation scenarios

### **Custom Exception Creation**
```csharp
public class CustomBusinessException : DomainException
{
    public string BusinessRule { get; }
    
    public CustomBusinessException(string businessRule, string message) 
        : base(message, "CUSTOM_BUSINESS_RULE", 422)
    {
        BusinessRule = businessRule;
    }
}
```

## 📋 ProblemDetails Middleware

### **RFC 7807 Compliance**
The middleware automatically maps exceptions to RFC 7807 ProblemDetails format with enhanced metadata.

### **Response Format**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Property Not Found",
  "status": 404,
  "detail": "Property with id '123' not found",
  "instance": "/properties/123",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "traceId": "0HMQ9VQJ2QK8P:00000001",
  "extensions": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "requestId": "0HMQ9VQJ2QK8P:00000001",
    "endpoint": "/properties/123",
    "method": "GET",
    "clientIp": "127.0.0.1"
  }
}
```

### **Automatic Exception Mapping**
```csharp
private static (int statusCode, string title, string? detail, string? type) MapExceptionToProblemDetails(Exception exception)
{
    return exception switch
    {
        PropertyNotFoundException propEx => (
            StatusCodes.Status404NotFound,
            "Property Not Found",
            propEx.Message,
            "https://tools.ietf.org/html/rfc7231#section-6.5.4"
        ),
        
        PropertyAlreadyActiveException => (
            StatusCodes.Status409Conflict,
            "Property Already Active",
            exception.Message,
            "https://tools.ietf.org/html/rfc7231#section-6.5.8"
        ),
        
        // ... more mappings
    };
}
```

### **HTTP Status Code Mapping**
| Exception Type | HTTP Status | RFC Reference |
|----------------|-------------|---------------|
| `PropertyNotFoundException` | 404 | RFC 7231 §6.5.4 |
| `PropertyAlreadyActiveException` | 409 | RFC 7231 §6.5.8 |
| `PropertyAlreadyInactiveException` | 409 | RFC 7231 §6.5.8 |
| `DomainValidationException` | 422 | RFC 4918 §11.2 |
| `BusinessRuleViolationException` | 422 | RFC 4918 §11.2 |
| `InvalidOperationException` | 400 | RFC 7231 §6.5.1 |
| `ArgumentException` | 400 | RFC 7231 §6.5.1 |
| `UnauthorizedAccessException` | 401 | RFC 7235 §3.1 |
| `TimeoutException` | 408 | RFC 7231 §6.5.7 |
| Generic `Exception` | 500 | RFC 7231 §6.6.1 |

## 🚦 Advanced Rate Limiting

### **Features**
- **Per-IP Rate Limiting** - Configurable requests per minute
- **Burst Protection** - Short-term spike handling
- **Endpoint-Specific Limits** - Different limits per endpoint
- **Intelligent IP Detection** - Support for proxy headers
- **Rich Headers** - Rate limit information in responses

### **Configuration**
```csharp
builder.Services.AddAdvancedRateLimiting(options =>
{
    options.DefaultLimitPerMinute = 120;        // 120 requests per minute
    options.BurstLimit = 200;                   // 200 requests in 10 seconds
    options.Window = TimeSpan.FromMinutes(1);  // 1 minute window
    options.EnableBurst = true;                 // Enable burst protection
});
```

### **Environment Variables**
```bash
RATE_LIMIT_PERMINUTE=120      # Default: 120
RATE_LIMIT_BURST=200          # Default: 200
RATE_LIMIT_ENABLE_BURST=true  # Default: true
```

### **Rate Limit Headers**
```http
X-RateLimit-Limit: 120
X-RateLimit-Remaining: 85
X-RateLimit-Reset: 1705312200
Retry-After: 60
```

### **Rate Limit Response**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Retry after 60 seconds.",
  "instance": "/properties",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "traceId": "0HMQ9VQJ2QK8P:00000001",
  "extensions": {
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "requestId": "0HMQ9VQJ2QK8P:00000001",
    "endpoint": "/properties",
    "method": "GET",
    "clientIp": "127.0.0.1"
  }
}
```

## 📊 Structured Logging

### **Request Lifecycle Logging**
```csharp
// Request Start
_logger.LogInformation("Request started: {Method} {Path} from {ClientIP}", 
    requestMethod, requestPath, clientIp);

// Request Success
_logger.LogInformation("Request completed successfully: {Method} {Path} - {StatusCode} in {ElapsedMs}ms", 
    requestMethod, requestPath, statusCode, elapsedMs);

// Request Warning (4xx status codes)
_logger.LogWarning("Request completed with warning: {Method} {Path} - {StatusCode} in {ElapsedMs}ms", 
    requestMethod, requestPath, statusCode, elapsedMs);

// Request Failure
_logger.LogError(ex, "Request failed: {Method} {Path} from {ClientIP} after {ElapsedMs}ms - {ErrorType}: {ErrorMessage}", 
    requestMethod, requestPath, clientIp, elapsedMs, ex.GetType().Name, ex.Message);
```

### **Log Context Enrichment**
```csharp
using var scope = _logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId,
    ["ClientIP"] = clientIp,
    ["RequestPath"] = requestPath,
    ["RequestMethod"] = requestMethod,
    ["UserAgent"] = userAgent
});
```

### **Log Output Example**
```json
{
  "Timestamp": "2024-01-15T10:30:00.000Z",
  "Level": "Information",
  "MessageTemplate": "Request started: {Method} {Path} from {ClientIP}",
  "Properties": {
    "Method": "GET",
    "Path": "/properties",
    "ClientIP": "127.0.0.1",
    "CorrelationId": "550e8400-e29b-41d4-a716-446655440000",
    "RequestPath": "/properties",
    "RequestMethod": "GET",
    "UserAgent": "PostmanRuntime/7.32.3"
  }
}
```

## 🔗 Correlation IDs

### **Request Tracing**
Every request gets a unique correlation ID that flows through the entire request lifecycle.

### **Header Usage**
```http
X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000
```

### **Auto-Generation**
If no correlation ID is provided, the system automatically generates a UUID v4.

### **Context Propagation**
```csharp
// Set in middleware
context.Items[CorrelationIdMiddleware.HttpContextItemKey] = correlationId;
context.Response.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;

// Include in all logs
using var scope = _logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId
});

// Include in error responses
var payload = new
{
    // ... other fields
    extensions = new { correlationId }
};
```

## 🛠️ Implementation Examples

### **Service Layer Error Handling**
```csharp
public async Task<PropertyDto> UpdatePropertyAsync(string id, UpdatePropertyRequest request, CancellationToken cancellationToken)
{
    var existingEntity = await _repository.GetByIdAsync(id, cancellationToken);
    if (existingEntity == null)
        throw new PropertyNotFoundException(id); // Automatically mapped to 404

    // Update logic...
    var updatedEntity = await _repository.UpdateAsync(id, existingEntity, cancellationToken);
    return MapToDto(updatedEntity);
}
```

### **Controller/Endpoint Error Handling**
```csharp
app.MapPut("/properties/{id}", async (
    string id,
    UpdatePropertyRequest request,
    IValidator<UpdatePropertyRequest> validator,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    // Validation
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        var detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status400BadRequest, "Validation Failed", detail);
        return Results.ValidationProblem(result.ToDictionary());
    }

    try
    {
        var updatedProperty = await service.UpdatePropertyAsync(id, request, ct);
        return Results.Ok(updatedProperty);
    }
    catch (PropertyNotFoundException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Resource Not Found", $"Property {id} not found");
        return Results.NotFound();
    }
    // Other exceptions are automatically handled by ProblemDetailsMiddleware
});
```

### **Custom Exception Handling**
```csharp
public class CustomExceptionHandler
{
    public async Task HandleAsync(HttpContext context, Exception exception)
    {
        if (exception is CustomBusinessException customEx)
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(
                context,
                StatusCodes.Status422UnprocessableEntity,
                "Business Rule Violation",
                customEx.Message,
                "https://example.com/problems/business-rule-violation");
        }
        else
        {
            // Let the global middleware handle it
            throw;
        }
    }
}
```

## 🧪 Testing Error Handling

### **Middleware Testing**
```csharp
[Test]
public async Task PropertyNotFoundException_maps_to_404()
{
    var context = new DefaultHttpContext();
    var middleware = new ProblemDetailsMiddleware(
        _ => throw new PropertyNotFoundException("test123"), 
        NullLogger<ProblemDetailsMiddleware>.Instance);
    
    await middleware.InvokeAsync(context);
    
    context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    context.Response.ContentType.Should().Be("application/problem+json");
}
```

### **Service Testing**
```csharp
[Test]
public async Task UpdateProperty_WithNonExistentId_ThrowsPropertyNotFoundException()
{
    // Arrange
    var request = new UpdatePropertyRequest { Name = "Updated" };
    
    // Act & Assert
    var ex = Assert.ThrowsAsync<PropertyNotFoundException>(async () =>
        await _service.UpdatePropertyAsync("nonexistent", request, CancellationToken.None));
    
    ex.PropertyId.Should().Be("nonexistent");
}
```

### **Integration Testing**
```csharp
[Test]
public async Task UpdateProperty_EndToEnd_Returns404ForNonExistent()
{
    // Arrange
    var request = new { Name = "Updated" };
    
    // Act
    var response = await _client.PutAsJsonAsync("/properties/nonexistent", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problemDetails.Status.Should().Be(404);
    problemDetails.Title.Should().Be("Property Not Found");
}
```

## 📈 Monitoring & Observability

### **Error Metrics**
- **Error Rate by Type** - Track frequency of different exception types
- **Response Time by Status** - Monitor performance impact of errors
- **Rate Limit Violations** - Track abuse patterns
- **Correlation ID Coverage** - Ensure all requests are traceable

### **Alerting**
```csharp
// High error rate alert
if (errorRate > 0.05) // 5% error rate
{
    _logger.LogError("High error rate detected: {ErrorRate}%", errorRate * 100);
    // Send alert
}

// Rate limit abuse alert
if (rateLimitViolations > 100) // 100 violations per minute
{
    _logger.LogWarning("High rate limit violations: {Violations}", rateLimitViolations);
    // Send alert
}
```

### **Dashboard Metrics**
- **Request Success Rate** - Overall API health
- **Error Distribution** - Breakdown by exception type
- **Response Time Percentiles** - Performance monitoring
- **Rate Limit Effectiveness** - Abuse prevention metrics

## 🔒 Security Considerations

### **Information Disclosure**
- **Production Errors** - Generic messages, no stack traces
- **Development Errors** - Detailed information for debugging
- **Sensitive Data** - Never log passwords, tokens, or PII

### **Rate Limiting Security**
- **IP Spoofing Protection** - Validate forwarded headers
- **Distributed Attacks** - Consider Redis-based rate limiting
- **Whitelist Support** - Allow trusted IPs to bypass limits

### **Log Security**
```csharp
// Sanitize sensitive data
var sanitizedHeaders = context.Request.Headers
    .Where(h => !h.Key.ToLower().Contains("authorization"))
    .ToDictionary(h => h.Key, h => h.Value);

_logger.LogInformation("Request headers: {Headers}", sanitizedHeaders);
```

## 🚀 Best Practices

### **Exception Design**
1. **Be Specific** - Create domain-specific exceptions
2. **Include Context** - Provide actionable error information
3. **Follow Naming** - Use descriptive, consistent names
4. **HTTP Mapping** - Map exceptions to appropriate status codes

### **Error Response Design**
1. **Consistent Format** - Always use ProblemDetails
2. **Actionable Messages** - Tell users how to fix the issue
3. **Include Context** - Provide relevant request information
4. **Traceability** - Always include correlation IDs

### **Logging Best Practices**
1. **Structured Logging** - Use structured data, not string concatenation
2. **Appropriate Levels** - Use correct log levels for different scenarios
3. **Performance Impact** - Minimize logging overhead
4. **Retention Policy** - Define log retention and archival strategy

### **Rate Limiting Best Practices**
1. **Gradual Backoff** - Implement exponential backoff for clients
2. **Clear Headers** - Provide rate limit information in responses
3. **Flexible Limits** - Different limits for different endpoints
4. **Monitoring** - Track rate limit effectiveness and abuse patterns

---

**This enhanced error handling system provides enterprise-grade reliability, observability, and user experience for the Million Properties API.**
