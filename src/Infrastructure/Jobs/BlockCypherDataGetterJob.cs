using System;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Infrastructure.Options;
using ICMarkets.Infrastructure.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ICMarkets.Infrastructure.Jobs;

/// <summary>
/// Periodically saves a snapshot of every supported pair. With several instances running only the one
/// holding the Redis lock polls, see <see cref="RedisLeaderLock"/>.
/// </summary>
public class BlockCypherDataGetterJob : BackgroundService
{
    private const string LockName = "icmarkets:jobs:blockcypher-data-getter";

    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IReferenceDataCache _referenceData;
    private readonly DataProviderOptions _options;
    private readonly ILogger<BlockCypherDataGetterJob> _logger;

    private const int _defaultInterval = 1;
    private const int _minLockSeconds = 3;

    public BlockCypherDataGetterJob(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        IReferenceDataCache referenceData,
        IOptions<DataProviderOptions> options,
        ILogger<BlockCypherDataGetterJob> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _referenceData = referenceData;
        _options = options?.Value ?? new DataProviderOptions();
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken st)
    {
        if (!_options.PeriodicDataRequestEnabled)
        {
            _logger.LogInformation("{ServiceName} is disabled.", GetType().Name);
            return;
        }

        var lockTtl = TimeSpan.FromSeconds(Math.Max(_minLockSeconds, _options.PeriodicDataRequestLockSeconds));
        var leaderLock = new RedisLeaderLock(
            new RedisLock(_redis, $"{LockName}:lock", lockTtl, _logger),
            new RedisPubSub(_redis, $"{LockName}:released", _logger),
            _logger);

        await leaderLock.RunAsync(PollAsync, st);
    }

    private async Task PollAsync(CancellationToken ct)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(
            _defaultInterval, 
            _options.PeriodicDataRequestIntervalSeconds));
        
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await RequestAllAsync(ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Periodic data request failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(ct));
    }

    private async Task RequestAllAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var client = scope.ServiceProvider.GetRequiredService<IBlockchainClient>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        foreach (var coinChain in _referenceData.CoinChains)
        {
            try
            {
                var info = await client.GetCoinChainInfoAsync(coinChain, ct);
                var latest = await unitOfWork.CoinChainInfos.GetLatestAsync(coinChain.Id, ct);

                if (info.HasSameDataAs(latest))
                {
                    _logger.LogDebug("No changes for {CoinChain} since the last snapshot, not saved.", coinChain);
                    continue;
                }

                unitOfWork.CoinChainInfos.Add(info);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Failed to get data for {CoinChain}.", coinChain);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
