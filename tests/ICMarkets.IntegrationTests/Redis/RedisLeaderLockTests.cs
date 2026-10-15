using System;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Infrastructure.Redis;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace ICMarkets.IntegrationTests.Redis;

public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder().WithImage("redis:7-alpine").Build();

    public IConnectionMultiplexer Redis { get; private set; }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Redis = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await Redis.DisposeAsync();
        await _container.DisposeAsync();
    }
}

public class RedisLeaderLockTests : IClassFixture<RedisFixture>
{
    // Long enough that a quick takeover can only come from the "released" message, not from expiry.
    private static readonly TimeSpan LongTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly IConnectionMultiplexer _redis;
    private readonly string _name = $"test:{Guid.NewGuid()}";

    public RedisLeaderLockTests(RedisFixture fixture)
    {
        _redis = fixture.Redis;
    }

    [Fact]
    public async Task Only_one_instance_runs_work()
    {
        using var first = Start(LongTtl);
        using var second = Start(LongTtl);

        var winner = await Task.WhenAny(first.Started.Task, second.Started.Task).WaitAsync(Timeout);
        await Task.Delay(500);

        Assert.False((winner == first.Started.Task ? second : first).Started.Task.IsCompleted);
    }

    [Fact]
    public async Task Waiting_instance_takes_over_when_owner_stops()
    {
        using var first = Start(LongTtl);
        await first.Started.Task.WaitAsync(Timeout);
        using var second = Start(LongTtl);
        await Task.Delay(500);
        Assert.False(second.Started.Task.IsCompleted);

        await first.StopAsync();

        await second.Started.Task.WaitAsync(Timeout);
        Assert.True(await _redis.GetDatabase().KeyExistsAsync($"{_name}:lock"));
    }

    [Fact]
    public async Task Owner_stop_releases_lock_and_publishes_message()
    {
        var published = new TaskCompletionSource();
        await _redis.GetSubscriber().SubscribeAsync(RedisChannel.Literal($"{_name}:released"), (_, _) => published.TrySetResult());
        using var owner = Start(LongTtl);
        await owner.Started.Task.WaitAsync(Timeout);

        await owner.StopAsync();

        await published.Task.WaitAsync(Timeout);
        Assert.False(await _redis.GetDatabase().KeyExistsAsync($"{_name}:lock"));
    }

    [Fact]
    public async Task Takes_over_after_expiry_when_owner_dies_without_release()
    {
        // Lock of a "killed" instance: never extended, never released, no message.
        await _redis.GetDatabase().LockTakeAsync($"{_name}:lock", "dead-instance", TimeSpan.FromSeconds(2));

        using var waiting = Start(TimeSpan.FromSeconds(2));
        await Task.Delay(500);
        Assert.False(waiting.Started.Task.IsCompleted);

        await waiting.Started.Task.WaitAsync(Timeout);
    }

    [Fact]
    public async Task Work_is_cancelled_when_lock_is_lost()
    {
        using var owner = Start(TimeSpan.FromSeconds(3));
        await owner.Started.Task.WaitAsync(Timeout);

        await _redis.GetDatabase().StringSetAsync($"{_name}:lock", "other-instance");

        await owner.WorkCancelled.Task.WaitAsync(Timeout);
    }

    private Runner Start(TimeSpan ttl)
    {
        var runner = new Runner();
        var leaderLock = new RedisLeaderLock(
            new RedisLock(_redis, $"{_name}:lock", ttl, NullLogger.Instance),
            new RedisPubSub(_redis, $"{_name}:released", NullLogger.Instance),
            NullLogger.Instance);

        runner.Run = leaderLock.RunAsync(async ct =>
        {
            runner.Started.TrySetResult();
            try
            {
                await Task.Delay(System.Threading.Timeout.Infinite, ct);
            }
            finally
            {
                runner.WorkCancelled.TrySetResult();
            }
        }, runner.Stop.Token);

        return runner;
    }

    private sealed class Runner : IDisposable
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource WorkCancelled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public CancellationTokenSource Stop { get; } = new();
        public Task Run { get; set; }

        public async Task StopAsync()
        {
            Stop.Cancel();
            await Run.WaitAsync(Timeout);
        }

        public void Dispose()
        {
            Stop.Cancel();
            Stop.Dispose();
        }
    }
}
