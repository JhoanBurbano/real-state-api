using Microsoft.AspNetCore.Http;

namespace Million.Web.Middlewares;

public class ConditionalProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public ConditionalProblemDetailsMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only execute ProblemDetailsMiddleware if NOT in Testing environment
        if (!_environment.IsEnvironment("Testing"))
        {
            // Execute the next middleware in the pipeline
            await _next(context);
        }
        else
        {
            // Skip ProblemDetailsMiddleware in Testing environment by calling next directly
            await _next(context);
        }
    }
}
