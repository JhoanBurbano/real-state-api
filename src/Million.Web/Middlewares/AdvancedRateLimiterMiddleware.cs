using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Million.Web.Middlewares;

public class RateLimitOptions
{
    public int DefaultLimitPerMinute { get; set; } = 120;
    public int BurstLimit { get; set; } = 200;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
    public bool EnableBurst { get; set; } = true;
}

public class AdvancedRateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdvancedRateLimiterMiddleware> _logger;
    private readonly RateLimitOptions _options;

    public AdvancedRateLimiterMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<AdvancedRateLimiterMiddleware> logger,
        IOptions<RateLimitOptions> options)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIp(context);
        var endpoint = context.Request.Path.Value ?? "/";

        if (!IsRateLimitAllowed(clientIp, endpoint))
        {
            await HandleRateLimitExceeded(context, clientIp, endpoint);
            return;
        }

        // Set rate limit headers for successful requests
        SetRateLimitHeaders(context, clientIp, endpoint);

        await _next(context);
    }

    private bool IsRateLimitAllowed(string clientIp, string endpoint)
    {
        var key = $"rate_limit_{clientIp}_{endpoint}";
        var burstKey = $"burst_{clientIp}_{endpoint}";

        // Check burst limit first
        if (_options.EnableBurst)
        {
            var burstCount = _cache.GetOrCreate(burstKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                return 0;
            });

            if (burstCount >= _options.BurstLimit)
            {
                _logger.LogWarning("Burst rate limit exceeded for {ClientIp} on {Endpoint}", clientIp, endpoint);
                return false;
            }

            _cache.Set(burstKey, burstCount + 1, TimeSpan.FromSeconds(10));
        }

        // Check regular rate limit
        var currentCount = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _options.Window;
            return 0;
        });

        if (currentCount >= _options.DefaultLimitPerMinute)
        {
            _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Endpoint}", clientIp, endpoint);
            return false;
        }

        _cache.Set(key, currentCount + 1, _options.Window);
        return true;
    }

    private void SetRateLimitHeaders(HttpContext context, string clientIp, string endpoint)
    {
        var key = $"rate_limit_{clientIp}_{endpoint}";
        var currentCount = _cache.Get<int>(key);
        var remaining = Math.Max(0, _options.DefaultLimitPerMinute - currentCount);
        var resetTime = DateTimeOffset.UtcNow.Add(_options.Window).ToUnixTimeSeconds();

        context.Response.Headers["X-RateLimit-Limit"] = _options.DefaultLimitPerMinute.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = resetTime.ToString();
    }

    private async Task HandleRateLimitExceeded(HttpContext context, string clientIp, string endpoint)
    {
        var retryAfter = _options.Window.TotalSeconds;
        context.Response.Headers["Retry-After"] = retryAfter.ToString();
        context.Response.Headers["X-RateLimit-Limit"] = _options.DefaultLimitPerMinute.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = "0";
        context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(_options.Window).ToUnixTimeSeconds().ToString();

        _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Endpoint}. Retry after {RetryAfter}s",
            clientIp, endpoint, retryAfter);

        await ProblemDetailsMiddleware.WriteProblemDetails(
            context,
            StatusCodes.Status429TooManyRequests,
            "Too Many Requests",
            $"Rate limit exceeded. Retry after {retryAfter} seconds.",
            "https://tools.ietf.org/html/rfc7231#section-6.5.8");
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

public static class AdvancedRateLimiterMiddlewareExtensions
{
    public static IServiceCollection AddAdvancedRateLimiting(this IServiceCollection services, Action<RateLimitOptions> configure)
    {
        services.Configure(configure);
        return services;
    }

    public static IApplicationBuilder UseAdvancedRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AdvancedRateLimiterMiddleware>();
    }
}
