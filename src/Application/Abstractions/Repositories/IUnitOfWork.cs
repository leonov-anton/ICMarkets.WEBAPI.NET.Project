using System.Threading;
using System.Threading.Tasks;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface IUnitOfWork
{
    ICoinRepository Coins { get; }
    IChainRepository Chains { get; }
    ICoinChainRepository CoinChains { get; }
    ICoinChainInfoRepository CoinChainInfos { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
