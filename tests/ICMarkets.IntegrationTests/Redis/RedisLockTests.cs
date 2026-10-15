using System;
using System.Threading.Tasks;
using ICMarkets.Infrastructure.Redis;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;

namespace ICMarkets.IntegrationTests.Redis;

public class RedisLockTests : IClassFixture<RedisFixture>
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly IConnectionMultiplexer _redis;
    private readonly string _key = $"test:{Guid.NewGuid()}:lock";

    public RedisLockTests(RedisFixture fixture)
    {
        _redis = fixture.Redis;
    }

    [Fact]
    public async Task Only_one_owner_can_acquire()
    {
        var first = new RedisLock(_redis, _key, Ttl, NullLogger.Instance);
        var second = new RedisLock(_redis, _key, Ttl, NullLogger.Instance);

        Assert.True(await first.TryAcquireAsync());
        Assert.False(await second.TryAcquireAsync());
    }

    [Fact]
    public async Task Only_owner_can_extend_and_release()
    {
        var owner = new RedisLock(_redis, _key, Ttl, NullLogger.Instance);
        var other = new RedisLock(_redis, _key, Ttl, NullLogger.Instance);
        await owner.TryAcquireAsync();

        Assert.False(await other.ExtendAsync());
        Assert.False(await other.ReleaseAsync());
        Assert.True(await owner.ExtendAsync());
        Assert.True(await owner.ReleaseAsync());
        Assert.True(await other.TryAcquireAsync());
    }

    [Fact]
    public async Task Lock_expires_after_ttl()
    {
        var owner = new RedisLock(_redis, _key, TimeSpan.FromMilliseconds(300), NullLogger.Instance);
        await owner.TryAcquireAsync();

        await Task.Delay(600);

        Assert.True(await new RedisLock(_redis, _key, Ttl, NullLogger.Instance).TryAcquireAsync());
        Assert.False(await owner.ExtendAsync());
    }
}
