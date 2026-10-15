using ICMarkets.Application.Queries.Response;
using ICMarkets.Domain.Entities;

namespace ICMarkets.UnitTests.Mapping;

public class ResponseMapperTests
{
    [Fact]
    public void Maps_coin_chain()
    {
        Assert.Equal(new CoinChainResponse(1, "BTC", "Main", "utxo"), TestData.BtcMain.ToResponse());
        Assert.Equal(new CoinChainResponse(7, "ETH", "Main", "evm"), TestData.EthMain.ToResponse());
    }

    [Fact]
    public void Maps_utxo_snapshot()
    {
        CoinChainInfo info = TestData.Utxo();

        var response = Assert.IsType<UtxoCoinChainInfoResponse>(info.ToResponse(TestData.BtcMain));

        Assert.Equal("BTC.Main", response.Name);
        Assert.Equal(info.Height, response.Height);
        Assert.Equal(info.Hash, response.Hash);
        Assert.Equal(info.Time, response.Time);
        Assert.Equal(info.LastForkHash, response.LastForkHash);
        Assert.Equal(info.CreatedAt, response.CreatedAt);
        Assert.Equal(3, response.HighFeePerKb);
        Assert.Equal(2, response.MediumFeePerKb);
        Assert.Equal(1, response.LowFeePerKb);
    }

    [Fact]
    public void Maps_evm_snapshot()
    {
        CoinChainInfo info = TestData.Evm();

        var response = Assert.IsType<EvmCoinChainInfoResponse>(info.ToResponse(TestData.EthMain));

        Assert.Equal("ETH.Main", response.Name);
        Assert.Equal(info.Height, response.Height);
        Assert.Equal(7, response.HighGasPrice);
        Assert.Equal(6, response.MediumGasPrice);
        Assert.Equal(5, response.LowGasPrice);
        Assert.Equal(4, response.HighPriorityFee);
        Assert.Equal(3, response.MediumPriorityFee);
        Assert.Equal(2, response.LowPriorityFee);
        Assert.Equal(1, response.BaseFee);
    }
}
