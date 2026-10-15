using System;
using ICMarkets.Domain.Entities;
using ICMarkets.Domain.Enums;

namespace ICMarkets.UnitTests;

internal static class TestData
{
    public static readonly CoinChain BtcMain =
        new(1, new Coin(1, "Bitcoin", "BTC"), new Chain(1, "Main"), ChainFamily.Utxo);

    public static readonly CoinChain EthMain =
        new(7, new Coin(6, "Ethereum", "ETH"), new Chain(1, "Main"), ChainFamily.Evm);

    public static UtxoCoinChainInfo Utxo(long height = 100) => new(
        BtcMain, height, "hash", new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc), "prev",
        peerCount: 10, unconfirmedCount: 20, lastForkHeight: 90, lastForkHash: "fork",
        highFeePerKb: 3, mediumFeePerKb: 2, lowFeePerKb: 1);

    public static EvmCoinChainInfo Evm(long height = 200) => new(
        EthMain, height, "hash", new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc), "prev",
        peerCount: 10, unconfirmedCount: 20, lastForkHeight: 190, lastForkHash: "fork",
        highGasPrice: 7, mediumGasPrice: 6, lowGasPrice: 5,
        highPriorityFee: 4, mediumPriorityFee: 3, lowPriorityFee: 2, baseFee: 1);
}
