using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.EF.Repositories;

public abstract class ReadRepository<TEntity, TKey> : IReadRepository<TEntity, TKey> where TEntity : class
{
    protected ReadRepository(BlockchainDbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    protected BlockchainDbContext Db { get; }
    protected DbSet<TEntity> Set { get; }

    public virtual async Task<TEntity> GetByIdAsync(TKey id, CancellationToken ct = default) =>
        await Set.FindAsync(new object[] { id }, ct);
}
