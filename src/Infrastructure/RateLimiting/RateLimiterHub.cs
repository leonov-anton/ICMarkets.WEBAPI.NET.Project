using System;
using System.Collections.Concurrent;

namespace ICMarkets.Infrastructure.RateLimiting;

public class RateLimiterHub : IRateLimiterHub
{
    private readonly ConcurrentDictionary<string, IRateLimiter> _limiters =
        new ConcurrentDictionary<string, IRateLimiter>(StringComparer.OrdinalIgnoreCase);

    public IRateLimiter GetOrAdd(string providerName, TimeSpan? interval, int? maxRequests)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("Provider name is required.", nameof(providerName));

        return _limiters.GetOrAdd(providerName, _ => new RateLimiter(interval, maxRequests));
    }
}
