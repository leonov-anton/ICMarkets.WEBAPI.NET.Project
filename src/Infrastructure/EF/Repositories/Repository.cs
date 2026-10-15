using ICMarkets.Application.Abstractions.Repositories;

namespace ICMarkets.Infrastructure.EF.Repositories;

public abstract class Repository<TEntity, TKey> : ReadRepository<TEntity, TKey>, IRepository<TEntity, TKey> where TEntity : class
{
    protected Repository(BlockchainDbContext db) : base(db)
    {
    }

    public virtual void Add(TEntity entity) => Set.Add(entity);

    public virtual void Remove(TEntity entity) => Set.Remove(entity);
}
