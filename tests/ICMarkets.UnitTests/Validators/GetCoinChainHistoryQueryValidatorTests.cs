using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Queries.Request;
using ICMarkets.Application.Queries.Response;
using ICMarkets.Application.Queries.Validators;
using ICMarkets.Domain.Entities;
using Mediator;
using NSubstitute;

namespace ICMarkets.UnitTests.Validators;

public class GetCoinChainHistoryQueryValidatorTests
{
    private readonly IPipelineBehavior<GetCoinChainHistoryQuery, IEnumerable<CoinChainInfoResponse>> _validator;
    private bool _nextCalled;

    public GetCoinChainHistoryQueryValidatorTests()
    {
        var referenceData = Substitute.For<IReferenceDataCache>();
        referenceData.TryGetCoinChain("btc", "main", out Arg.Any<CoinChain>()).Returns(true);

        _validator = new GetCoinChainHistoryQueryValidator(referenceData);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(50)]
    public async Task Valid_query_reaches_handler(int? pageSize)
    {
        await Validate(new GetCoinChainHistoryQuery("btc", "main", null, pageSize));

        Assert.True(_nextCalled);
    }

    [Theory]
    [InlineData(null, "main")]
    [InlineData(" ", "main")]
    [InlineData("btc", null)]
    [InlineData("btc", "")]
    public async Task Missing_coin_or_chain_is_rejected(string coin, string chain)
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Validate(new GetCoinChainHistoryQuery(coin, chain, null)));

        Assert.False(_nextCalled);
    }

    [Theory]
    [InlineData("xyz", "main")]
    [InlineData("btc", "xyz")]
    public async Task Unsupported_pair_is_rejected(string coin, string chain)
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => Validate(new GetCoinChainHistoryQuery(coin, chain, null)));

        Assert.Contains("not supported", ex.Message);
        Assert.False(_nextCalled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Non_positive_page_size_is_rejected(int pageSize)
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Validate(new GetCoinChainHistoryQuery("btc", "main", null, pageSize)));

        Assert.False(_nextCalled);
    }

    private async Task Validate(GetCoinChainHistoryQuery query) =>
        await _validator.Handle(query, (_, _) =>
        {
            _nextCalled = true;
            return new ValueTask<IEnumerable<CoinChainInfoResponse>>([]);
        }, CancellationToken.None);
}
