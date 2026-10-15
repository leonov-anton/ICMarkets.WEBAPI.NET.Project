using System;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Infrastructure.Redis;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;

namespace ICMarkets.IntegrationTests.Redis;

public class RedisPubSubTests : IClassFixture<RedisFixture>
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly RedisPubSub _pubSub;

    public RedisPubSubTests(RedisFixture fixture)
    {
        _pubSub = new RedisPubSub(fixture.Redis, $"test:{Guid.NewGuid()}", NullLogger.Instance);
    }

    [Fact]
    public async Task Subscriber_receives_published_message()
    {
        await using var subscription = await _pubSub.SubscribeAsync();

        var received = subscription.WaitAsync(Timeout, CancellationToken.None);
        await _pubSub.PublishAsync("hello");

        Assert.Equal("hello", await received);
    }

    [Fact]
    public async Task Message_published_before_wait_is_not_lost()
    {
        await using var subscription = await _pubSub.SubscribeAsync();

        await _pubSub.PublishAsync("early");
        await Task.Delay(200);

        Assert.Equal("early", await subscription.WaitAsync(Timeout, CancellationToken.None));
    }

    [Fact]
    public async Task Wait_returns_null_on_timeout()
    {
        await using var subscription = await _pubSub.SubscribeAsync();

        Assert.Null(await subscription.WaitAsync(TimeSpan.FromMilliseconds(200), CancellationToken.None));
    }

    [Fact]
    public async Task Wait_throws_when_cancelled()
    {
        await using var subscription = await _pubSub.SubscribeAsync();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => subscription.WaitAsync(Timeout, cts.Token));
    }
}
