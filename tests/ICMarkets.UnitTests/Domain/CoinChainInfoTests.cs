using System;
using ICMarkets.Domain.Entities;

namespace ICMarkets.UnitTests.Domain;

public class CoinChainInfoTests
{
    private static readonly DateTime Time = new(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Same_data_is_detected_regardless_of_created_at()
    {
        Assert.True(TestData.Utxo().HasSameDataAs(TestData.Utxo()));
        Assert.True(TestData.Evm().HasSameDataAs(TestData.Evm()));
    }

    [Fact]
    public void Nothing_to_compare_with_means_changed()
    {
        Assert.False(TestData.Utxo().HasSameDataAs(null));
    }

    [Fact]
    public void Time_differing_below_microsecond_is_same()
    {
        var original = Utxo(time: Time.AddTicks(7));
        var fromDatabase = Utxo(time: Time);

        Assert.True(original.HasSameDataAs(fromDatabase));
    }

    [Fact]
    public void Changed_common_field_is_detected()
    {
        var latest = Utxo();

        Assert.False(Utxo(height: 101).HasSameDataAs(latest));
        Assert.False(Utxo(unconfirmedCount: 21).HasSameDataAs(latest));
        Assert.False(Utxo(time: Time.AddSeconds(1)).HasSameDataAs(latest));
    }

    [Fact]
    public void Changed_utxo_fee_is_detected()
    {
        Assert.False(Utxo(mediumFeePerKb: 5).HasSameDataAs(Utxo()));
    }

    [Fact]
    public void Changed_evm_fee_is_detected()
    {
        var latest = TestData.Evm();
        var changed = new EvmCoinChainInfo(
            TestData.EthMain, 200, "hash", Time, "prev", 10, 20, 190, "fork",
            7, 6, 5, 4, 3, 2, baseFee: 99);

        Assert.False(changed.HasSameDataAs(latest));
    }

    [Fact]
    public void Different_pairs_are_not_same()
    {
        var btcTestnet = new CoinChain(2, TestData.BtcMain.Coin, new Chain(2, "Testnet3"), TestData.BtcMain.Family);
        var other = new UtxoCoinChainInfo(btcTestnet, 100, "hash", Time, "prev", 10, 20, 90, "fork", 3, 2, 1);

        Assert.False(other.HasSameDataAs(Utxo()));
    }

    private static UtxoCoinChainInfo Utxo(
        long height = 100, DateTime? time = null, int unconfirmedCount = 20, long mediumFeePerKb = 2) => new(
        TestData.BtcMain, height, "hash", time ?? Time, "prev",
        peerCount: 10, unconfirmedCount: unconfirmedCount, lastForkHeight: 90, lastForkHash: "fork",
        highFeePerKb: 3, mediumFeePerKb: mediumFeePerKb, lowFeePerKb: 1);
}
