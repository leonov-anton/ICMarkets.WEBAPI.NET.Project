using System;
using System.Collections.Generic;
using ICMarkets.Application.Queries.Response;
using Mediator;

namespace ICMarkets.Application.Queries.Request;

public record GetCoinChainHistoryQuery(
    string Coin, 
    string Chain, 
    DateTime? LatestData, 
    int? PageSize = 100) : 
    IQuery<IEnumerable<CoinChainInfoResponse>>;
