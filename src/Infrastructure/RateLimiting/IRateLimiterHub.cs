using System;

namespace ICMarkets.Infrastructure.RateLimiting;

public interface IRateLimiterHub
{
    IRateLimiter GetOrAdd(string providerName, TimeSpan? interval, int? maxRequests);
}
