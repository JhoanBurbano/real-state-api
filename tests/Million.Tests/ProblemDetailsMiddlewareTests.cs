using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Million.Domain.Exceptions;
using Million.Web.Middlewares;
using System.Text.Json;

namespace Million.Tests;

public class ProblemDetailsMiddlewareTests
{
    [Test]
    public async Task Unhandled_exception_results_in_problem_details_500()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new Exception("boom"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task PropertyNotFoundException_maps_to_404()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new PropertyNotFoundException("test123"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task PropertyAlreadyActiveException_maps_to_409()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new PropertyAlreadyActiveException("test123"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task PropertyAlreadyInactiveException_maps_to_409()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new PropertyAlreadyInactiveException("test123"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task DomainValidationException_maps_to_422()
    {
        var context = new DefaultHttpContext();
        var validationErrors = new Dictionary<string, string[]> { ["field"] = new[] { "error" } };
        var middleware = new ProblemDetailsMiddleware(_ => throw new DomainValidationException(validationErrors), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task BusinessRuleViolationException_maps_to_422()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new BusinessRuleViolationException("rule", "violation"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task InvalidOperationException_maps_to_400()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new InvalidOperationException("invalid"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task ArgumentException_maps_to_400()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new ArgumentException("argument"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task UnauthorizedAccessException_maps_to_401()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new UnauthorizedAccessException("unauthorized"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task TimeoutException_maps_to_408()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new TimeoutException("timeout"), NullLogger<ProblemDetailsMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status408RequestTimeout);
        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task WriteProblemDetails_creates_proper_response()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Request.Method = "GET";
        context.TraceIdentifier = "trace123";
        context.Response.Body = new MemoryStream();

        await ProblemDetailsMiddleware.WriteProblemDetails(context, 400, "Test Error", "Test detail");

        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().Be("application/problem+json");

        // Read response body
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("title").GetString().Should().Be("Test Error");
        problemDetails.GetProperty("detail").GetString().Should().Be("Test detail");
        problemDetails.GetProperty("instance").GetString().Should().Be("/test");
        problemDetails.GetProperty("traceId").GetString().Should().Be("trace123");
        problemDetails.GetProperty("extensions").GetProperty("endpoint").GetString().Should().Be("/test");
        problemDetails.GetProperty("extensions").GetProperty("method").GetString().Should().Be("GET");
    }

    [Test]
    public async Task WriteProblemDetails_with_custom_type_uses_provided_type()
    {
        var context = new DefaultHttpContext();
        var customType = "https://example.com/custom-error";
        context.Response.Body = new MemoryStream();

        await ProblemDetailsMiddleware.WriteProblemDetails(context, 400, "Test Error", "Test detail", customType);

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("type").GetString().Should().Be(customType);
    }

    [Test]
    public async Task WriteProblemDetails_includes_timestamp()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await ProblemDetailsMiddleware.WriteProblemDetails(context, 400, "Test Error");

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("timestamp").GetString().Should().NotBeNullOrEmpty();
        DateTimeOffset.TryParse(problemDetails.GetProperty("timestamp").GetString(), out _).Should().BeTrue();
    }
}

