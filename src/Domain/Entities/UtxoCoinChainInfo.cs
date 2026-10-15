using System;
using ICMarkets.Domain.Enums;

namespace ICMarkets.Domain.Entities;

/// <summary>Snapshot of a BTC-like chain; fees are in satoshis (or the coin's smallest unit) per kilobyte.</summary>
public class UtxoCoinChainInfo : CoinChainInfo
{
    public long HighFeePerKb { get; }
    public long MediumFeePerKb { get; }
    public long LowFeePerKb { get; }

    // For EF Core materialization.
    private UtxoCoinChainInfo()
    {
    }

    public UtxoCoinChainInfo(
        CoinChain coinChain,
        long height,
        string hash,
        DateTime time,
        string previousHash,
        int peerCount,
        int unconfirmedCount,
        long lastForkHeight,
        string lastForkHash,
        long highFeePerKb,
        long mediumFeePerKb,
        long lowFeePerKb)
        : base(coinChain, ChainFamily.Utxo, height, hash, time, previousHash, peerCount, unconfirmedCount, lastForkHeight, lastForkHash)
    {
        HighFeePerKb = highFeePerKb;
        MediumFeePerKb = mediumFeePerKb;
        LowFeePerKb = lowFeePerKb;
    }

    public override bool HasSameDataAs(CoinChainInfo other) =>
        base.HasSameDataAs(other)
        && other is UtxoCoinChainInfo utxo
        && HighFeePerKb == utxo.HighFeePerKb
        && MediumFeePerKb == utxo.MediumFeePerKb
        && LowFeePerKb == utxo.LowFeePerKb;
}
