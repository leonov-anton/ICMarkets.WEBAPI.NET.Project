using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ICMarkets.Infrastructure.Redis;

public class RedisLeaderLock
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly RedisLock _lock;
    private readonly RedisPubSub _released;
    private readonly ILogger _logger;

    public RedisLeaderLock(RedisLock redisLock, RedisPubSub released, ILogger logger)
    {
        _lock = redisLock;
        _released = released;
        _logger = logger;
    }

    public async Task RunAsync(Func<CancellationToken, Task> work, CancellationToken ct)
    {
        await using var subscription = await _released.SubscribeAsync();

        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (await _lock.TryAcquireAsync())
                    {
                        _logger.LogInformation("Lock {LockKey} acquired by {Owner}.", _lock.Key, _lock.Owner);
                        await RunAsOwnerAsync(work, ct);
                    }
                    else
                    {
                        _logger.LogInformation("Lock {LockKey} is held by another instance, waiting for it to be released.", _lock.Key);
                        await subscription.WaitAsync(_lock.Ttl, ct);
                    }
                }
                catch (RedisException ex)
                {
                    _logger.LogError(ex, "Redis is unavailable, retrying in {RetryDelay}.", RetryDelay);
                    await subscription.WaitAsync(RetryDelay, ct);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
    }

    private async Task RunAsOwnerAsync(Func<CancellationToken, Task> work, CancellationToken ct)
    {
        using var ownership = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var renewal = KeepLockAsync(ownership);

        try
        {
            await work(ownership.Token);
        }
        catch (OperationCanceledException) when (ownership.IsCancellationRequested)
        {
        }
        finally
        {
            ownership.Cancel();
            await renewal;
            await ReleaseAsync();
        }
    }

    private async Task KeepLockAsync(CancellationTokenSource ownership)
    {
        using var timer = new PeriodicTimer(_lock.Ttl / 3);

        try
        {
            while (await timer.WaitForNextTickAsync(ownership.Token))
            {
                if (!await _lock.ExtendAsync())
                {
                    _logger.LogWarning("Lock {LockKey} was lost by {Owner}, stopping work.", _lock.Key, _lock.Owner);
                    ownership.Cancel();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Failed to extend lock {LockKey}, stopping work.", _lock.Key);
            ownership.Cancel();
        }
    }

    private async Task ReleaseAsync()
    {
        try
        {
            if (await _lock.ReleaseAsync())
            {
                await _released.PublishAsync(_lock.Owner);
                _logger.LogInformation("Lock {LockKey} released by {Owner}.", _lock.Key, _lock.Owner);
            }
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Failed to release lock {LockKey}, it expires in at most {LockTtl}.", _lock.Key, _lock.Ttl);
        }
    }
}
