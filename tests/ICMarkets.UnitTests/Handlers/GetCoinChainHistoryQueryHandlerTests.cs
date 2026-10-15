using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Application.Queries.Handlers;
using ICMarkets.Application.Queries.Request;
using ICMarkets.Application.Queries.Response;
using ICMarkets.Domain.Entities;
using NSubstitute;

namespace ICMarkets.UnitTests.Handlers;

public class GetCoinChainHistoryQueryHandlerTests
{
    private readonly ICoinChainInfoRepository _infos = Substitute.For<ICoinChainInfoRepository>();
    private readonly GetCoinChainHistoryQueryHandler _handler;

    public GetCoinChainHistoryQueryHandlerTests()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.CoinChainInfos.Returns(_infos);

        var referenceData = Substitute.For<IReferenceDataCache>();
        referenceData.TryGetCoinChain("btc", "main", out Arg.Any<CoinChain>())
            .Returns(x => { x[2] = TestData.BtcMain; return true; });

        _handler = new GetCoinChainHistoryQueryHandler(
            unitOfWork, Substitute.For<IBlockchainClient>(), referenceData);
    }

    [Fact]
    public async Task Passes_paging_to_repository_and_maps_result()
    {
        var latest = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
        _infos.GetHistoryAsync(TestData.BtcMain.Id, 2, latest, Arg.Any<CancellationToken>())
            .Returns(new List<CoinChainInfo> { TestData.Utxo(101), TestData.Utxo(100) });

        var result = (await _handler.Handle(new GetCoinChainHistoryQuery("btc", "main", latest, 2), default)).ToList();

        Assert.Equal([101L, 100L], result.Select(x => x.Height));
        Assert.All(result, x => Assert.Equal("BTC.Main", x.Name));
    }

    [Fact]
    public async Task Uses_default_page_size_when_not_provided()
    {
        _infos.GetHistoryAsync(default, default, default, default).ReturnsForAnyArgs(new List<CoinChainInfo>());

        await _handler.Handle(new GetCoinChainHistoryQuery("btc", "main", null, null), default);

        await _infos.Received(1).GetHistoryAsync(TestData.BtcMain.Id, 100, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_null_for_unknown_pair()
    {
        var result = await _handler.Handle(new GetCoinChainHistoryQuery("xyz", "main", null), default);

        Assert.Null(result);
        await _infos.DidNotReceiveWithAnyArgs().GetHistoryAsync(default, default, default, default);
    }
}
