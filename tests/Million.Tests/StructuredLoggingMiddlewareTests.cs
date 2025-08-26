using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Million.Web.Middlewares;
using NSubstitute;
using FluentAssertions;

namespace Million.Tests;

public class StructuredLoggingMiddlewareTests
{
    private ILogger<StructuredLoggingMiddleware> _logger;

    [SetUp]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<StructuredLoggingMiddleware>>();
    }

    [Test]
    public async Task Successful_request_logs_start_and_completion()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Request.Method = "GET";
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context.Response.StatusCode = 200;

        var middleware = new StructuredLoggingMiddleware(_ => Task.CompletedTask, _logger);

        await middleware.InvokeAsync(context);

        // Verify that logging methods were called
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public async Task Failed_request_logs_start_and_failure()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Request.Method = "GET";
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        var exception = new Exception("Test exception");
        var middleware = new StructuredLoggingMiddleware(_ => throw exception, _logger);

        // Act & Assert
        var thrownException = Assert.ThrowsAsync<Exception>(async () => await middleware.InvokeAsync(context));

        // Verify that error logging was called
        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public async Task Warning_status_code_logs_warning()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Request.Method = "GET";
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context.Response.StatusCode = 404;

        var middleware = new StructuredLoggingMiddleware(_ => Task.CompletedTask, _logger);

        await middleware.InvokeAsync(context);

        // Verify that warning logging was called
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public async Task Uses_forwarded_for_header_when_present()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Request.Method = "GET";
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.100";
        context.Response.StatusCode = 200;

        var middleware = new StructuredLoggingMiddleware(_ => Task.CompletedTask, _logger);

        await middleware.InvokeAsync(context);

        // Verify that logging was called
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
