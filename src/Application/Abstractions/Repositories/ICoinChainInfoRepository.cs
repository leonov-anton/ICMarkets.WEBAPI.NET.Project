using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Domain.Entities;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface ICoinChainInfoRepository : IRepository<CoinChainInfo, long>
{
    Task<CoinChainInfo> GetLatestAsync(int coinChainId, CancellationToken ct = default);

    /// <summary>
    /// Snapshots of one pair, newest first. For the next page pass CreatedAt of the last item
    /// of the previous page as <paramref name="before"/>, or null for the first page.
    /// </summary>
    Task<IReadOnlyList<CoinChainInfo>> GetHistoryAsync(
        int coinChainId,
        int limit,
        DateTime? before = null,
        CancellationToken ct = default);
}
