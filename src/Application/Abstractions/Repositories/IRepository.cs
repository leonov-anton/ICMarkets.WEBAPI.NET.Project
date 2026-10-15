namespace ICMarkets.Application.Abstractions.Repositories;

public interface IRepository<TEntity, in TKey> : IReadRepository<TEntity, TKey> where TEntity : class
{
    void Add(TEntity entity);

    void Remove(TEntity entity);
}
