using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Domain.Entities;

namespace ICMarkets.Application.Abstractions.Repositories;

/// <summary>Read-only: pairs are added directly in the database (and picked up on restart).</summary>
public interface ICoinChainRepository : IReadRepository<CoinChain, int>
{
    Task<IReadOnlyList<CoinChain>> GetAllAsync(CancellationToken ct = default);
}
