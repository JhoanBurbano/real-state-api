using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Million.Web.Middlewares;
using FluentAssertions;

namespace Million.Tests;

public class AdvancedRateLimiterMiddlewareTests
{
    private IMemoryCache _cache;
    private ILogger<AdvancedRateLimiterMiddleware> _logger;
    private RateLimitOptions _options;

    [SetUp]
    public void Setup()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _logger = NullLogger<AdvancedRateLimiterMiddleware>.Instance;
        _options = new RateLimitOptions
        {
            DefaultLimitPerMinute = 5,
            BurstLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            EnableBurst = true
        };
    }

    [TearDown]
    public void TearDown()
    {
        _cache?.Dispose();
    }

    [Test]
    public async Task Rate_limit_allowed_when_under_limit()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context.Request.Path = "/test";

        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task Rate_limit_exceeded_when_over_limit()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context.Request.Path = "/test";

        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        // Make requests up to the limit
        for (int i = 0; i < _options.DefaultLimitPerMinute; i++)
        {
            await middleware.InvokeAsync(context);
        }

        // This request should be rate limited
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(429);
        context.Response.Headers["Retry-After"].Should().Contain("60");
        context.Response.Headers["X-RateLimit-Limit"].Should().Contain("5");
        context.Response.Headers["X-RateLimit-Remaining"].Should().Contain("0");
    }

    [Test]
    public async Task Burst_limit_enforced_when_enabled()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context.Request.Path = "/test";

        _options.EnableBurst = true;
        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        // Make requests up to the burst limit
        for (int i = 0; i < _options.BurstLimit; i++)
        {
            await middleware.InvokeAsync(context);
        }

        // This request should be rate limited by burst
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(429);
    }

    [Test]
    public async Task Burst_limit_ignored_when_disabled()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context.Request.Path = "/test";

        _options.EnableBurst = false;
        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        // Make requests up to the regular limit (should not be enforced by burst)
        for (int i = 0; i < _options.DefaultLimitPerMinute; i++)
        {
            await middleware.InvokeAsync(context);
        }

        // This should still work since we're within the regular limit
        context.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task Different_endpoints_have_separate_rate_limits()
    {
        var context1 = new DefaultHttpContext();
        context1.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context1.Request.Path = "/endpoint1";

        var context2 = new DefaultHttpContext();
        context2.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context2.Request.Path = "/endpoint2";

        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        // Fill up endpoint1
        for (int i = 0; i < _options.DefaultLimitPerMinute; i++)
        {
            await middleware.InvokeAsync(context1);
        }

        // Endpoint2 should still work
        await middleware.InvokeAsync(context2);
        context2.Response.StatusCode.Should().Be(200);

        // Endpoint1 should be rate limited
        await middleware.InvokeAsync(context1);
        context1.Response.StatusCode.Should().Be(429);
    }

    [Test]
    public async Task Different_ips_have_separate_rate_limits()
    {
        var context1 = new DefaultHttpContext();
        context1.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context1.Request.Path = "/test";

        var context2 = new DefaultHttpContext();
        context2.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.2");
        context2.Request.Path = "/test";

        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        // Fill up IP1
        for (int i = 0; i < _options.DefaultLimitPerMinute; i++)
        {
            await middleware.InvokeAsync(context1);
        }

        // IP2 should still work
        await middleware.InvokeAsync(context2);
        context2.Response.StatusCode.Should().Be(200);

        // IP1 should be rate limited
        await middleware.InvokeAsync(context1);
        context1.Response.StatusCode.Should().Be(429);
    }

    [Test]
    public async Task X_Forwarded_For_header_used_when_present()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.100";
        context.Request.Path = "/test";

        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task X_Real_IP_header_used_when_present()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Real-IP"] = "10.0.0.100";
        context.Request.Path = "/test";

        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    [Test]
    public async Task Rate_limit_headers_included_in_response()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context.Request.Path = "/test";

        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-RateLimit-Limit"].Should().Contain("5");
        context.Response.Headers["X-RateLimit-Remaining"].Should().Contain("4");
        context.Response.Headers["X-RateLimit-Reset"].Should().NotBeEmpty();
    }

    [Test]
    public async Task Rate_limit_reset_time_is_future()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        context.Request.Path = "/test";

        var middleware = new AdvancedRateLimiterMiddleware(
            _ => Task.CompletedTask,
            _cache,
            _logger,
            Options.Create(_options));

        await middleware.InvokeAsync(context);

        var resetTime = long.Parse(context.Response.Headers["X-RateLimit-Reset"].First()!);
        var resetDateTime = DateTimeOffset.FromUnixTimeSeconds(resetTime);

        resetDateTime.Should().BeAfter(DateTimeOffset.UtcNow);
    }
}
