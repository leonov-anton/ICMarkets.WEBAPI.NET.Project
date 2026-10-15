using System;
using ICMarkets.Domain.Enums;

namespace ICMarkets.Domain.Entities;

/// <summary>Snapshot of an Ethereum-like chain; all prices are in wei.</summary>
public class EvmCoinChainInfo : CoinChainInfo
{
    public long HighGasPrice { get; }
    public long MediumGasPrice { get; }
    public long LowGasPrice { get; }
    public long HighPriorityFee { get; }
    public long MediumPriorityFee { get; }
    public long LowPriorityFee { get; }
    public long BaseFee { get; }

    // For EF Core materialization.
    private EvmCoinChainInfo()
    {
    }

    public EvmCoinChainInfo(
        CoinChain coinChain,
        long height,
        string hash,
        DateTime time,
        string previousHash,
        int peerCount,
        int unconfirmedCount,
        long lastForkHeight,
        string lastForkHash,
        long highGasPrice,
        long mediumGasPrice,
        long lowGasPrice,
        long highPriorityFee,
        long mediumPriorityFee,
        long lowPriorityFee,
        long baseFee)
        : base(coinChain, ChainFamily.Evm, height, hash, time, previousHash, peerCount, unconfirmedCount, lastForkHeight, lastForkHash)
    {
        HighGasPrice = highGasPrice;
        MediumGasPrice = mediumGasPrice;
        LowGasPrice = lowGasPrice;
        HighPriorityFee = highPriorityFee;
        MediumPriorityFee = mediumPriorityFee;
        LowPriorityFee = lowPriorityFee;
        BaseFee = baseFee;
    }

    public override bool HasSameDataAs(CoinChainInfo other) =>
        base.HasSameDataAs(other)
        && other is EvmCoinChainInfo evm
        && HighGasPrice == evm.HighGasPrice
        && MediumGasPrice == evm.MediumGasPrice
        && LowGasPrice == evm.LowGasPrice
        && HighPriorityFee == evm.HighPriorityFee
        && MediumPriorityFee == evm.MediumPriorityFee
        && LowPriorityFee == evm.LowPriorityFee
        && BaseFee == evm.BaseFee;
}
