using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Million.Domain.Exceptions;

namespace Million.Web.Middlewares;

public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, type) = MapExceptionToProblemDetails(exception);

        _logger.LogError(exception, "Unhandled exception: {ExceptionType} - {Message} for {Path} from {ClientIP}",
            exception.GetType().Name,
            exception.Message,
            context.Request.Path,
            GetClientIp(context));

        await WriteProblemDetails(context, statusCode, title, detail, type);
    }

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

            PropertyAlreadyInactiveException => (
                StatusCodes.Status409Conflict,
                "Property Already Inactive",
                exception.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            ),

            DomainValidationException domainEx => (
                StatusCodes.Status422UnprocessableEntity,
                "Domain Validation Failed",
                exception.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),

            BusinessRuleViolationException businessEx => (
                StatusCodes.Status422UnprocessableEntity,
                "Business Rule Violation",
                exception.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),

            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Invalid Operation",
                exception.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),

            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid Argument",
                exception.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                exception.Message,
                "https://tools.ietf.org/html/rfc7235#section-3.1"
            ),

            TimeoutException => (
                StatusCodes.Status408RequestTimeout,
                "Request Timeout",
                exception.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.7"
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            )
        };
    }

    public static async Task WriteProblemDetails(HttpContext context, int status, string title, string? detail = null, string? type = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var correlationId = GetCorrelationId(context);
        var timestamp = DateTimeOffset.UtcNow;

        var payload = new
        {
            type = type ?? GetDefaultType(status),
            title,
            status,
            detail,
            instance = context.Request.Path.ToString(),
            timestamp = timestamp.ToString("O"),
            traceId = context.TraceIdentifier,
            extensions = new
            {
                correlationId,
                requestId = context.TraceIdentifier,
                endpoint = context.Request.Path.Value,
                method = context.Request.Method,
                clientIp = GetClientIp(context)
            }
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false // Always false for consistency
        });

        await context.Response.WriteAsync(json);
    }

    private static string GetDefaultType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status408RequestTimeout => "https://tools.ietf.org/html/rfc7231#section-6.5.7",
        StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
        StatusCodes.Status429TooManyRequests => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        StatusCodes.Status502BadGateway => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
        StatusCodes.Status503ServiceUnavailable => "https://tools.ietf.org/html/rfc7231#section-6.6.4",
        StatusCodes.Status504GatewayTimeout => "https://tools.ietf.org/html/rfc7231#section-6.6.5",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.5.1"
    };

    private static string? GetCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdMiddleware.HttpContextItemKey, out var idObj))
        {
            return idObj?.ToString();
        }

        return context.Request.Headers[CorrelationIdMiddleware.HeaderName].FirstOrDefault();
    }

    private static string GetClientIp(HttpContext context)
    {
        // Check for forwarded headers first
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

