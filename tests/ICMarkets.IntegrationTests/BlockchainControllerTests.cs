using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ICMarkets.Application.Queries.Response;
using ICMarkets.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ICMarkets.IntegrationTests;

public class BlockchainControllerTests : IClassFixture<ApiFactory>
{
    private static readonly DateTime BaseTime = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public BlockchainControllerTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Coins_returns_seeded_coins()
    {
        var coins = await _client.GetFromJsonAsync<List<NamedEntity<int>>>("api/blockchain/coins");

        Assert.Equal(["Bitcoin", "Dash", "Dogecoin", "Litecoin", "BlockCypher", "Ethereum"], coins.Select(x => x.Name));
    }

    [Fact]
    public async Task Chains_returns_seeded_chains()
    {
        var chains = await _client.GetFromJsonAsync<List<NamedEntity<int>>>("api/blockchain/chain");

        Assert.Equal(["Main", "Testnet3", "Test"], chains.Select(x => x.Name));
    }

    [Fact]
    public async Task Pairs_returns_seeded_coin_chain_pairs()
    {
        var pairs = await _client.GetFromJsonAsync<List<CoinChainResponse>>("api/blockchain/pairs");

        Assert.Equal(
            ["BTC.Main", "BTC.Testnet3", "DASH.Main", "DOGE.Main", "LTC.Main", "BCY.Test", "ETH.Main"],
            pairs.Select(x => $"{x.Coin}.{x.Chain}"));
        Assert.Equal("evm", pairs.Single(x => x.Coin == "ETH").Family);
    }

    [Fact]
    public async Task History_returns_snapshots_of_pair_newest_first()
    {
        var btc = _factory.GetCoinChain("BTC", "Main");
        var eth = _factory.GetCoinChain("ETH", "Main");
        await _factory.SeedSnapshotsAsync(
            (Utxo(btc, 1), BaseTime),
            (Utxo(btc, 2), BaseTime.AddMinutes(1)),
            (Evm(eth, 3), BaseTime.AddMinutes(2)));

        var history = await GetHistory("api/blockchain/btc/main");

        Assert.Equal([2L, 1L], history.Select(x => x.Height));
        var latest = Assert.IsType<UtxoCoinChainInfoResponse>(history[0]);
        Assert.Equal("BTC.Main", latest.Name);
        Assert.Equal(30, latest.HighFeePerKb);
    }

    [Fact]
    public async Task History_of_evm_pair_contains_gas_fields()
    {
        var eth = _factory.GetCoinChain("ETH", "Main");
        await _factory.SeedSnapshotsAsync((Evm(eth, 5), BaseTime));

        var history = await GetHistory("api/blockchain/eth/main");

        var info = Assert.IsType<EvmCoinChainInfoResponse>(Assert.Single(history));
        Assert.Equal(70, info.HighGasPrice);
        Assert.Equal(10, info.BaseFee);
    }

    [Fact]
    public async Task History_is_paged_by_page_size_and_latest()
    {
        var btc = _factory.GetCoinChain("BTC", "Main");
        await _factory.SeedSnapshotsAsync(Enumerable.Range(1, 5)
            .Select(i => ((CoinChainInfo)Utxo(btc, i), BaseTime.AddMinutes(i)))
            .ToArray());

        var firstPage = await GetHistory("api/blockchain/btc/main?pageSize=2");
        var secondPage = await GetHistory(
            $"api/blockchain/btc/main?pageSize=2&latest={Uri.EscapeDataString(firstPage[^1].CreatedAt.ToString("O"))}");

        Assert.Equal([5L, 4L], firstPage.Select(x => x.Height));
        Assert.Equal([3L, 2L], secondPage.Select(x => x.Height));
    }

    [Theory]
    [InlineData("api/blockchain/xyz/main")]
    [InlineData("api/blockchain/btc/xyz")]
    [InlineData("api/blockchain/eth/testnet3")]
    [InlineData("api/blockchain/btc/main?pageSize=0")]
    public async Task Invalid_request_returns_400(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(400, problem.Status);
        Assert.False(string.IsNullOrEmpty(problem.Detail));
    }

    [Fact]
    public async Task Correlation_id_from_request_is_returned()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/blockchain/coins");
        request.Headers.Add("X-Correlation-ID", "test-id");

        var response = await _client.SendAsync(request);

        Assert.Equal("test-id", Assert.Single(response.Headers.GetValues("X-Correlation-ID")));
    }

    [Fact]
    public async Task Correlation_id_is_generated_when_missing()
    {
        var response = await _client.GetAsync("api/blockchain/xyz/main");

        Assert.True(Guid.TryParse(Assert.Single(response.Headers.GetValues("X-Correlation-ID")), out _));
    }

    private async Task<List<CoinChainInfoResponse>> GetHistory(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return await response.Content.ReadFromJsonAsync<List<CoinChainInfoResponse>>(
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static UtxoCoinChainInfo Utxo(CoinChain coinChain, long height) => new(
        coinChain, height, $"hash{height}", BaseTime, "prev", 10, 20, 0, null, 30, 20, 10);

    private static EvmCoinChainInfo Evm(CoinChain coinChain, long height) => new(
        coinChain, height, $"hash{height}", BaseTime, "prev", 10, 20, 0, null, 70, 60, 50, 40, 30, 20, 10);
}
