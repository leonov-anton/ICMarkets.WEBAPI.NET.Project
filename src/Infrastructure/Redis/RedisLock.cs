using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ICMarkets.Infrastructure.Redis;

/// <summary>
/// Lock on a Redis key with a lifetime. Only the instance that took it can extend or release it.
/// </summary>
public class RedisLock
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger _logger;

    public RedisKey Key { get; }
    public TimeSpan Ttl { get; }
    public string Owner { get; } = $"{Environment.MachineName}:{Guid.NewGuid()}";

    public RedisLock(IConnectionMultiplexer redis, string key, TimeSpan ttl, ILogger logger)
    {
        _redis = redis;
        Key = key;
        Ttl = ttl;
        _logger = logger;
    }

    public async Task<bool> TryAcquireAsync()
    {
        var acquired = await _redis.GetDatabase().LockTakeAsync(Key, Owner, Ttl);

        if (acquired)
            _logger.LogDebug("Lock {LockKey} acquired by {Owner} for {LockTtl}.", Key, Owner, Ttl);
        else
            _logger.LogDebug("Lock {LockKey} not acquired by {Owner}: held by another owner.", Key, Owner);

        return acquired;
    }

    /// <summary>Returns false if the lock is no longer owned by this instance.</summary>
    public async Task<bool> ExtendAsync()
    {
        var extended = await _redis.GetDatabase().LockExtendAsync(Key, Owner, Ttl);

        if (extended)
            _logger.LogDebug("Lock {LockKey} extended by {Owner} for {LockTtl}.", Key, Owner, Ttl);
        else
            _logger.LogDebug("Lock {LockKey} not extended: no longer owned by {Owner}.", Key, Owner);

        return extended;
    }

    /// <summary>Returns false if the lock is no longer owned by this instance.</summary>
    public async Task<bool> ReleaseAsync()
    {
        var released = await _redis.GetDatabase().LockReleaseAsync(Key, Owner);

        if (released)
            _logger.LogDebug("Lock {LockKey} released by {Owner}.", Key, Owner);
        else
            _logger.LogDebug("Lock {LockKey} not released: no longer owned by {Owner}.", Key, Owner);

        return released;
    }
}
