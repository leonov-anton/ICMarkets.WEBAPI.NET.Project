using System;

namespace ICMarkets.Infrastructure.Options;

public class DataProviderOptions
{
    public string BaseUrl { get; set; }
    public int? RateLimitRequests { get; set; }
    public int? RateLimitIntervalMilliseconds { get; set; }
    public bool PeriodicDataRequestEnabled { get; set; }
    public int PeriodicDataRequestIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Lifetime of the Redis lock that lets only one instance poll. If the polling instance dies without
    /// releasing it, another one takes over after this time.
    /// </summary>
    public int PeriodicDataRequestLockSeconds { get; set; } = 30;

    public TimeSpan? RateLimitInterval =>
        RateLimitIntervalMilliseconds > 0 ? TimeSpan.FromMilliseconds(RateLimitIntervalMilliseconds.Value) : null;
}
