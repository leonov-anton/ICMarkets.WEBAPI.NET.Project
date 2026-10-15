using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Infrastructure.RateLimiting;

namespace ICMarkets.UnitTests.RateLimiting;

public class RateLimiterTests
{
    private static readonly TimeSpan LongInterval = TimeSpan.FromMinutes(1);

    [Fact]
    public void Allows_max_requests_without_waiting_then_blocks()
    {
        var limiter = new RateLimiter(LongInterval, maxRequests: 3);
        using var cts = new CancellationTokenSource();

        var allowed = Enumerable.Range(0, 3).Select(_ => limiter.WaitAsync(cts.Token)).ToList();
        var blocked = limiter.WaitAsync(cts.Token);

        Assert.All(allowed, t => Assert.True(t.IsCompletedSuccessfully));
        Assert.False(blocked.IsCompleted);
        cts.Cancel();
    }

    [Fact]
    public void Blocked_request_is_scheduled_one_interval_after_oldest()
    {
        var limiter = new RateLimiter(LongInterval, maxRequests: 2);
        var before = DateTimeOffset.UtcNow;

        limiter.WaitAsync();
        limiter.WaitAsync();

        Assert.InRange(limiter.NextRequestAt, before + LongInterval, DateTimeOffset.UtcNow + LongInterval);
    }

    [Fact]
    public async Task Request_proceeds_once_window_frees()
    {
        var limiter = new RateLimiter(TimeSpan.FromMilliseconds(200), maxRequests: 1);

        await limiter.WaitAsync();
        var waited = System.Diagnostics.Stopwatch.StartNew();
        await limiter.WaitAsync();

        Assert.True(waited.ElapsedMilliseconds >= 150, $"Waited only {waited.ElapsedMilliseconds} ms.");
    }

    [Fact]
    public void Missing_max_requests_means_one_per_interval()
    {
        var limiter = new RateLimiter(LongInterval, maxRequests: null);
        using var cts = new CancellationTokenSource();

        Assert.True(limiter.WaitAsync(cts.Token).IsCompletedSuccessfully);
        Assert.False(limiter.WaitAsync(cts.Token).IsCompleted);
        cts.Cancel();
    }

    [Fact]
    public void Without_interval_never_waits()
    {
        var limiter = new RateLimiter(interval: null, maxRequests: 1);

        Assert.All(Enumerable.Range(0, 10), _ => Assert.True(limiter.WaitAsync().IsCompletedSuccessfully));
    }

    [Fact]
    public void DelayUntil_blocks_requests_even_without_interval()
    {
        var limiter = new RateLimiter(interval: null, maxRequests: null);
        using var cts = new CancellationTokenSource();
        var until = DateTimeOffset.UtcNow + LongInterval;

        limiter.DelayUntil(until);

        Assert.Equal(until, limiter.NextRequestAt);
        Assert.False(limiter.WaitAsync(cts.Token).IsCompleted);
        cts.Cancel();
    }

    [Fact]
    public void DelayUntil_ignores_earlier_time()
    {
        var limiter = new RateLimiter(interval: null, maxRequests: null);
        var until = DateTimeOffset.UtcNow + LongInterval;

        limiter.DelayUntil(until);
        limiter.DelayUntil(until - TimeSpan.FromSeconds(30));

        Assert.Equal(until, limiter.NextRequestAt);
    }
}
