using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions;
using ICMarkets.Domain.Entities;
using ICMarkets.Domain.Enums;
using ICMarkets.Infrastructure.BlockCypher.Models.Response;
using ICMarkets.Infrastructure.Options;
using ICMarkets.Infrastructure.RateLimiting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using RestEase;

namespace ICMarkets.Infrastructure.BlockCypher;

public class BlockCypherBlockchainClient : IBlockchainClient
{
    private const string ProviderName = "BlockCypher";
    private const int MaxRetries = 3;
    private static readonly TimeSpan DefaultBackoff = TimeSpan.FromSeconds(1);

    private readonly IBlockCypherApi _api;
    private readonly IRateLimiter _rateLimiter;
    private ResiliencePipeline _pipeline;

    public BlockCypherBlockchainClient(
        IBlockCypherApi api,
        IRateLimiterHub rateLimiters,
        IOptions<DataProviderOptions> options)
    {
        _api = api;
        _rateLimiter = rateLimiters.GetOrAdd(
            ProviderName, options?.Value?.RateLimitInterval, options?.Value?.RateLimitRequests);

        SetPipeLine();
    }

    public async Task<CoinChainInfo> GetCoinChainInfoAsync(CoinChain coinChain, CancellationToken ct = default)
    {
        var r = await RequestAsync(coinChain, ct);

        return coinChain.Family switch
        {
            ChainFamily.Utxo => new UtxoCoinChainInfo(
                coinChain, r.Height, r.Hash, r.Time, r.PreviousHash, r.PeerCount, r.UnconfirmedCount,
                r.LastForkHeight, r.LastForkHash,
                r.HighFeePerKb, r.MediumFeePerKb, r.LowFeePerKb),

            ChainFamily.Evm => new EvmCoinChainInfo(
                coinChain, r.Height, r.Hash, r.Time, r.PreviousHash, r.PeerCount, r.UnconfirmedCount,
                r.LastForkHeight, r.LastForkHash,
                r.HighGasPrice, r.MediumGasPrice, r.LowGasPrice,
                r.HighPriorityFee, r.MediumPriorityFee, r.LowPriorityFee, r.BaseFee),

            _ => throw new NotSupportedException($"Chain family {coinChain.Family} is not supported."),
        };
    }

    private Task<BlockCypherChainInfoResponse> RequestAsync(CoinChain coinChain, CancellationToken ct)
    {
        var coin = coinChain.Coin.ShortName.ToLower();
        var chain = coinChain.Chain.Name.ToLower();

        return _pipeline.ExecuteAsync(async token =>
        {
            await _rateLimiter.WaitAsync(token);
            return await _api.GetChainInfoAsync(coin, chain, token);
        }, ct).AsTask();
    }

    private static TimeSpan? GetRetryAfter(ApiException ex)
    {
        var retryAfter = ex.Headers?.RetryAfter;
        if (retryAfter?.Delta is TimeSpan delta) return delta;
        if (retryAfter?.Date is DateTimeOffset date) return date - DateTimeOffset.UtcNow;
        return null;
    }

    private void SetPipeLine()
    {
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = MaxRetries,
                Delay = TimeSpan.Zero,
                ShouldHandle = new PredicateBuilder()
                    .Handle<ApiException>(e => e.StatusCode == HttpStatusCode.TooManyRequests),
                DelayGenerator = args =>
                {
                    var ex = (ApiException)args.Outcome.Exception;
                    var backoff = GetRetryAfter(ex) ?? _rateLimiter.Interval ?? DefaultBackoff;
                    _rateLimiter.DelayUntil(DateTimeOffset.UtcNow + backoff);
                    return new ValueTask<TimeSpan?>(TimeSpan.Zero);
                }
            })
            .Build();
    }
}
