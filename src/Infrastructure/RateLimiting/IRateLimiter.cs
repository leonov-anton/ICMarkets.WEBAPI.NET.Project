using System;
using System.Threading;
using System.Threading.Tasks;

namespace ICMarkets.Infrastructure.RateLimiting;

public interface IRateLimiter
{
    TimeSpan? Interval { get; }
    DateTimeOffset NextRequestAt { get; }
    Task WaitAsync(CancellationToken ct = default);
    void DelayUntil(DateTimeOffset time);
}
