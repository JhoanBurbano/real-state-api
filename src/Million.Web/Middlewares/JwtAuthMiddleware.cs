using Microsoft.Extensions.Primitives;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Domain.Exceptions;

namespace Million.Web.Middlewares;

public class JwtAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuthService _authService;

    public JwtAuthMiddleware(RequestDelegate next, IAuthService authService)
    {
        _next = next;
        _authService = authService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        // Check if endpoint requires authentication
        var requiresAuth = endpoint.Metadata.GetMetadata<RequiresAuthAttribute>();
        if (requiresAuth == null)
        {
            await _next(context);
            return;
        }

        // Extract token from Authorization header
        var token = ExtractToken(context);
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ProblemDetailsMiddleware.WriteProblemDetails(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Missing or invalid authorization token");
            return;
        }

        try
        {
            // Validate token and get owner
            var owner = await _authService.GetOwnerFromTokenAsync(token, context.RequestAborted);
            if (owner == null || !owner.IsActive)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ProblemDetailsMiddleware.WriteProblemDetails(
                    context,
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "Invalid or expired token");
                return;
            }

            // Check role requirements
            if (requiresAuth.RequireAdmin && owner.Role != OwnerRole.Admin)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await ProblemDetailsMiddleware.WriteProblemDetails(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "Insufficient permissions");
                return;
            }

            // Add owner to context for downstream use
            context.Items["Owner"] = owner;
            context.Items["OwnerId"] = owner.Id;
            context.Items["OwnerRole"] = owner.Role;

            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ProblemDetailsMiddleware.WriteProblemDetails(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Token validation failed");
        }
    }

    private static string? ExtractToken(HttpContext context)
    {
        // Check Authorization header
        if (context.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
        {
            var authValue = authHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(authValue) && authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authValue.Substring("Bearer ".Length);
            }
        }

        // Check cookie (alternative approach)
        if (context.Request.Cookies.TryGetValue("access_token", out var cookieToken))
        {
            return cookieToken;
        }

        return null;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresAuthAttribute : Attribute
{
    public bool RequireAdmin { get; set; }

    public RequiresAuthAttribute(bool requireAdmin = false)
    {
        RequireAdmin = requireAdmin;
    }
}

public static class JwtAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtAuth(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtAuthMiddleware>();
    }
}

