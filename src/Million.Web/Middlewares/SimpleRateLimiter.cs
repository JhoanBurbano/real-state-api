using Microsoft.Extensions.Caching.Memory;

namespace Million.Web.Middlewares;

public class SimpleRateLimiter
{
    private readonly IMemoryCache _cache;

    public SimpleRateLimiter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool Allow(string clientKey, int limitPerMinute)
    {
        var key = $"rl:{clientKey}:{DateTime.UtcNow:yyyyMMddHHmm}";
        var count = _cache.GetOrCreate<int>(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });
        if (count >= limitPerMinute) return false;
        _cache.Set(key, count + 1, TimeSpan.FromMinutes(1));
        return true;
    }
}

