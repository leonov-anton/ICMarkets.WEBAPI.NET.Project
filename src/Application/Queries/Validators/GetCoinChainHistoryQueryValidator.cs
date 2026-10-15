using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Queries.Request;
using ICMarkets.Application.Queries.Response;
using Mediator;

namespace ICMarkets.Application.Queries.Validators;

public sealed class GetCoinChainHistoryQueryValidator :
    MessagePreProcessor<GetCoinChainHistoryQuery, IEnumerable<CoinChainInfoResponse>>
{
    private readonly IReferenceDataCache _referenceData;

    public GetCoinChainHistoryQueryValidator(IReferenceDataCache referenceData)
    {
        _referenceData = referenceData;
    }

    protected override ValueTask Handle(GetCoinChainHistoryQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Coin))
            throw new ValidationException("Coin is required.");

        if (string.IsNullOrWhiteSpace(query.Chain))
            throw new ValidationException("Chain is required.");

        if (!_referenceData.TryGetCoinChain(query.Coin, query.Chain, out _))
            throw new ValidationException($"Coin '{query.Coin}' on chain '{query.Chain}' is not supported.");

        if (query.PageSize <= 0)
            throw new ValidationException("PageSize must be greater than zero.");

        return default;
    }
}
