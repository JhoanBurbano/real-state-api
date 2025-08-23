using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Million.Web.Middlewares;

public class StructuredLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StructuredLoggingMiddleware> _logger;

    public StructuredLoggingMiddleware(RequestDelegate next, ILogger<StructuredLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = GetCorrelationId(context);
        var clientIp = GetClientIp(context);
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        var requestPath = context.Request.Path.Value ?? "/";
        var requestMethod = context.Request.Method ?? "UNKNOWN";

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["ClientIP"] = clientIp,
            ["RequestPath"] = requestPath,
            ["RequestMethod"] = requestMethod,
            ["UserAgent"] = userAgent
        });

        try
        {
            _logger.LogInformation("Request started: {Method} {Path} from {ClientIP}",
                requestMethod, requestPath, clientIp);

            await _next(context);

            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 400)
            {
                _logger.LogWarning("Request completed with warning: {Method} {Path} - {StatusCode} in {ElapsedMs}ms",
                    requestMethod, requestPath, statusCode, elapsedMs);
            }
            else
            {
                _logger.LogInformation("Request completed successfully: {Method} {Path} - {StatusCode} in {ElapsedMs}ms",
                    requestMethod, requestPath, statusCode, elapsedMs);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            _logger.LogError(ex, "Request failed: {Method} {Path} from {ClientIP} after {ElapsedMs}ms - {ErrorType}: {ErrorMessage}",
                requestMethod, requestPath, clientIp, elapsedMs, ex.GetType().Name, ex.Message);

            throw; // Re-throw to let ProblemDetailsMiddleware handle it
        }
    }

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

public static class StructuredLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseStructuredLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<StructuredLoggingMiddleware>();
    }
}
