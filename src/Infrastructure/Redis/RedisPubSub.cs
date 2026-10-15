using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ICMarkets.Infrastructure.Redis;

public class RedisPubSub
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger _logger;

    public RedisChannel Channel { get; }

    public RedisPubSub(IConnectionMultiplexer redis, string channel, ILogger logger)
    {
        _redis = redis;
        Channel = RedisChannel.Literal(channel);
        _logger = logger;
    }

    public async Task PublishAsync(string message)
    {
        var receivers = await _redis.GetSubscriber().PublishAsync(Channel, message);

        _logger.LogDebug("Published {Message} to {Channel}, received by {Receivers} subscribers.", message, Channel, receivers);
    }

    public async Task<Subscription> SubscribeAsync()
    {
        var queue = await _redis.GetSubscriber().SubscribeAsync(Channel);

        _logger.LogDebug("Subscribed to {Channel}.", Channel);
        return new Subscription(queue, _logger);
    }

    public sealed class Subscription : IAsyncDisposable
    {
        private readonly ChannelMessageQueue _queue;
        private readonly ILogger _logger;
        private readonly Channel<string> _latest = System.Threading.Channels.Channel.CreateBounded<string>(
            new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });

        internal Subscription(ChannelMessageQueue queue, ILogger logger)
        {
            _queue = queue;
            _logger = logger;
            _queue.OnMessage(message =>
            {
                _logger.LogDebug("Received {Message} from {Channel}.", (string)message.Message, _queue.Channel);
                _latest.Writer.TryWrite(message.Message);
            });
        }

        public async Task<string> WaitAsync(TimeSpan timeout, CancellationToken ct)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(timeout);

            try
            {
                return await _latest.Reader.ReadAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _logger.LogDebug("No message from {Channel} within {Timeout}.", _queue.Channel, timeout);
                return null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _queue.UnsubscribeAsync();

            _logger.LogDebug("Unsubscribed from {Channel}.", _queue.Channel);
        }
    }
}
