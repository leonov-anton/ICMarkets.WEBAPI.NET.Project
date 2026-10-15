using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.EF.Repositories;

// Read queries are no-tracking: snapshots are immutable and the table is large.
public class CoinChainInfoRepository : Repository<CoinChainInfo, long>, ICoinChainInfoRepository
{
    public CoinChainInfoRepository(BlockchainDbContext db) : base(db)
    {
    }

    public Task<CoinChainInfo> GetLatestAsync(int coinChainId, CancellationToken ct = default) =>
        Set.AsNoTracking()
            .Where(x => x.CoinChainId == coinChainId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CoinChainInfo>> GetHistoryAsync(
        int coinChainId,
        int limit,
        DateTime? before = null,
        CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Where(x => x.CoinChainId == coinChainId);

        if (before.HasValue)
            query = query.Where(x => x.CreatedAt < before.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }
}
