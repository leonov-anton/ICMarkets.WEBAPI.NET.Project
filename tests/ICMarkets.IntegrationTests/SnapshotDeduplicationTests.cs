using System;
using System.Threading.Tasks;
using ICMarkets.Domain.Entities;
using ICMarkets.Infrastructure.EF;

namespace ICMarkets.IntegrationTests;

// The job compares a fresh snapshot with the latest one read back from PostgreSQL.
public class SnapshotDeduplicationTests : IClassFixture<ApiFactory>
{
    // 7 digits after the second: more precise than PostgreSQL keeps.
    private static readonly DateTime BlockTime = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc).AddTicks(1234567);

    private readonly ApiFactory _factory;

    public SnapshotDeduplicationTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Snapshot_read_back_from_database_has_same_data_as_fresh_one()
    {
        var btc = _factory.GetCoinChain("BTC", "Main");
        await _factory.SeedSnapshotsAsync((Utxo(btc), DateTime.UtcNow));

        var latest = await GetLatestAsync(btc);

        Assert.True(Utxo(btc).HasSameDataAs(latest));
    }

    [Fact]
    public async Task Evm_snapshot_read_back_from_database_has_same_data_as_fresh_one()
    {
        var eth = _factory.GetCoinChain("ETH", "Main");
        await _factory.SeedSnapshotsAsync((Evm(eth), DateTime.UtcNow));

        var latest = await GetLatestAsync(eth);

        Assert.True(Evm(eth).HasSameDataAs(latest));
    }

    private async Task<CoinChainInfo> GetLatestAsync(CoinChain coinChain)
    {
        await using var db = _factory.CreateDbContext();
        return await new UnitOfWork(db).CoinChainInfos.GetLatestAsync(coinChain.Id);
    }

    private static UtxoCoinChainInfo Utxo(CoinChain coinChain) => new(
        coinChain, 100, "hash", BlockTime, "prev", 10, 20, 90, null, 30, 20, 10);

    private static EvmCoinChainInfo Evm(CoinChain coinChain) => new(
        coinChain, 200, "hash", BlockTime, null, 10, 20, 0, null, 70, 60, 50, 40, 30, 20, 10);
}
