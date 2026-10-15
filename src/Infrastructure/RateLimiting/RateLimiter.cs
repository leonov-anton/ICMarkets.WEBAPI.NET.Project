using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ICMarkets.Infrastructure.RateLimiting;

public class RateLimiter : IRateLimiter
{
    private readonly object _sync = new object();
    private readonly Queue<DateTimeOffset> _slots = new Queue<DateTimeOffset>();
    private DateTimeOffset _blockedUntil = DateTimeOffset.MinValue;

    public TimeSpan? Interval { get; }
    public int MaxRequests { get; }

    public RateLimiter(TimeSpan? interval, int? maxRequests)
    {
        Interval = interval > TimeSpan.Zero ? interval : null;
        MaxRequests = maxRequests > 0 ? maxRequests.Value : 1;
    }

    public DateTimeOffset NextRequestAt
    {
        get
        {
            lock (_sync)
            {
                return Interval.HasValue && _slots.Count >= MaxRequests
                    ? Max(_blockedUntil, _slots.Peek() + Interval.Value)
                    : _blockedUntil;
            }
        }
    }

    public async Task WaitAsync(CancellationToken ct = default)
    {
        TimeSpan delay;

        lock (_sync)
        {
            var now = DateTimeOffset.UtcNow;
            var start = Max(now, _blockedUntil);

            if (Interval.HasValue)
            {
                // Window is full: wait until the oldest of the last MaxRequests requests leaves it.
                if (_slots.Count >= MaxRequests)
                    start = Max(start, _slots.Dequeue() + Interval.Value);

                _slots.Enqueue(start);
            }

            delay = start - now;
        }

        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, ct);
    }

    public void DelayUntil(DateTimeOffset time)
    {
        lock (_sync)
        {
            if (time > _blockedUntil)
                _blockedUntil = time;
        }
    }

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;
}
