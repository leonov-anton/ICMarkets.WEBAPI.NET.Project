using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.EF.Repositories;

public class CoinRepository : ReadRepository<Coin, int>, ICoinRepository
{
    public CoinRepository(BlockchainDbContext db) : base(db)
    {
    }

    public async Task<IReadOnlyList<Coin>> GetAllAsync(CancellationToken ct = default) =>
        await Set.OrderBy(x => x.Id).ToListAsync(ct);
}
