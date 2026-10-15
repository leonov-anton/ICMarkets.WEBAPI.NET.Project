using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Domain.Entities;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface ICoinRepository : IReadRepository<Coin, int>
{
    Task<IReadOnlyList<Coin>> GetAllAsync(CancellationToken ct = default);
}
