using System;
using ICMarkets.Domain.Entities;

namespace ICMarkets.Application.Queries.Response;

public static class ResponseMapper
{
    // Family in the same form as the "family" discriminator of snapshot responses.
    public static CoinChainResponse ToResponse(this CoinChain coinChain) => new(
        coinChain.Id,
        coinChain.Coin.ShortName,
        coinChain.Chain.Name,
        coinChain.Family.ToString().ToLowerInvariant());

    // coinChain comes from IReferenceDataCache: snapshots store only CoinChainId.
    public static CoinChainInfoResponse ToResponse(this CoinChainInfo info, CoinChain coinChain) => info switch
    {
        UtxoCoinChainInfo utxo => utxo.ToResponse(coinChain),
        EvmCoinChainInfo evm => evm.ToResponse(coinChain),
        _ => throw new NotSupportedException($"Snapshot type {info?.GetType().Name} is not supported."),
    };

    public static UtxoCoinChainInfoResponse ToResponse(this UtxoCoinChainInfo info, CoinChain coinChain) => new()
    {
        Name = coinChain.ToString(),
        Height = info.Height,
        Hash = info.Hash,
        Time = info.Time,
        PreviousHash = info.PreviousHash,
        PeerCount = info.PeerCount,
        UnconfirmedCount = info.UnconfirmedCount,
        LastForkHeight = info.LastForkHeight,
        LastForkHash = info.LastForkHash,
        CreatedAt = info.CreatedAt,
        HighFeePerKb = info.HighFeePerKb,
        MediumFeePerKb = info.MediumFeePerKb,
        LowFeePerKb = info.LowFeePerKb,
    };

    public static EvmCoinChainInfoResponse ToResponse(this EvmCoinChainInfo info, CoinChain coinChain) => new()
    {
        Name = coinChain.ToString(),
        Height = info.Height,
        Hash = info.Hash,
        Time = info.Time,
        PreviousHash = info.PreviousHash,
        PeerCount = info.PeerCount,
        UnconfirmedCount = info.UnconfirmedCount,
        LastForkHeight = info.LastForkHeight,
        LastForkHash = info.LastForkHash,
        CreatedAt = info.CreatedAt,
        HighGasPrice = info.HighGasPrice,
        MediumGasPrice = info.MediumGasPrice,
        LowGasPrice = info.LowGasPrice,
        HighPriorityFee = info.HighPriorityFee,
        MediumPriorityFee = info.MediumPriorityFee,
        LowPriorityFee = info.LowPriorityFee,
        BaseFee = info.BaseFee,
    };
}
