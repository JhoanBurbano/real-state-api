using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Million.Web.Middlewares;

namespace Million.Tests;

public class CorrelationIdMiddlewareTests
{
    [Test]
    public async Task Should_set_correlation_id_header_when_missing()
    {
        var http = new DefaultHttpContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(http);
        http.Response.Headers.ContainsKey(CorrelationIdMiddleware.HeaderName).Should().BeTrue();
        http.Items.ContainsKey(CorrelationIdMiddleware.HttpContextItemKey).Should().BeTrue();
    }
}

