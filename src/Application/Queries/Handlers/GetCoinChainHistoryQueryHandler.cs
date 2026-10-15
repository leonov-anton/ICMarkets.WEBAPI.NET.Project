using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Application.Queries.Request;
using ICMarkets.Application.Queries.Response;
using ICMarkets.Domain.Entities;
using Mediator;

namespace ICMarkets.Application.Queries.Handlers;

public class GetCoinChainHistoryQueryHandler : 
    IQueryHandler<GetCoinChainHistoryQuery, IEnumerable<CoinChainInfoResponse>>
{
    private const int DefaultPageSize = 100;

    private readonly IUnitOfWork _unitOtWork;
    private readonly IBlockchainClient _blockChainClient;
    private readonly IReferenceDataCache _referenceData;

    public GetCoinChainHistoryQueryHandler(
        IUnitOfWork unitOtWork,
        IBlockchainClient blockChainClient, 
        IReferenceDataCache referenceData)
    {
        _unitOtWork = unitOtWork;
        _blockChainClient = blockChainClient;
        _referenceData = referenceData;
    }

    public async ValueTask<IEnumerable<CoinChainInfoResponse>> Handle(
        GetCoinChainHistoryQuery query, 
        CancellationToken ct)
    {
        if (!_referenceData.TryGetCoinChain(query.Coin, query.Chain, out CoinChain coinChain))
        {
            return null;
        }

        var res = await _unitOtWork.CoinChainInfos.GetHistoryAsync(
            coinChain.Id, 
            query.PageSize ?? DefaultPageSize, 
            query.LatestData, 
            ct);

        return res.Select(x => x.ToResponse(coinChain)).ToList();
    }
}
