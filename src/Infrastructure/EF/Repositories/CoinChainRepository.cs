using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.EF.Repositories;

public class CoinChainRepository : ReadRepository<CoinChain, int>, ICoinChainRepository
{
    public CoinChainRepository(BlockchainDbContext db) : base(db)
    {
    }

    public override Task<CoinChain> GetByIdAsync(int id, CancellationToken ct = default) =>
        WithReferences().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<CoinChain>> GetAllAsync(CancellationToken ct = default) =>
        await WithReferences().OrderBy(x => x.Id).ToListAsync(ct);

    private IQueryable<CoinChain> WithReferences() =>
        Set.Include(x => x.Coin).Include(x => x.Chain);
}
