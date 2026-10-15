using System.Threading;
using System.Threading.Tasks;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface IReadRepository<TEntity, in TKey> where TEntity : class
{
    Task<TEntity> GetByIdAsync(TKey id, CancellationToken ct = default);
}
