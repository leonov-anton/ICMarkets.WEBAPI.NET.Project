using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Domain.Entities;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface IChainRepository : IReadRepository<Chain, int>
{
    Task<IReadOnlyList<Chain>> GetAllAsync(CancellationToken ct = default);
}
