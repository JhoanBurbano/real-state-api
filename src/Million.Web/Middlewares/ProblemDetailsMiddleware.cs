using System.Net;
using System.Text.Json;

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
            _logger.LogError(ex, "Unhandled error");
            await WriteProblemDetails(context, (int)HttpStatusCode.InternalServerError, "Unexpected Error", ex.Message);
        }
    }

    public static async Task WriteProblemDetails(HttpContext context, int status, string title, string? detail = null, string? type = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;
        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.HttpContextItemKey, out var idObj) ? idObj?.ToString() : null;
        var payload = new
        {
            type = type ?? status switch
            {
                StatusCodes.Status400BadRequest => "https://example.com/problems/validation-error",
                StatusCodes.Status404NotFound => "https://example.com/problems/not-found",
                StatusCodes.Status429TooManyRequests => "https://example.com/problems/too-many-requests",
                _ => "https://example.com/problems/unexpected-error"
            },
            title,
            status,
            detail,
            instance = context.Request.Path.ToString(),
            extensions = new { correlationId }
        };
        var json = JsonSerializer.Serialize(payload);
        await context.Response.WriteAsync(json);
    }
}

