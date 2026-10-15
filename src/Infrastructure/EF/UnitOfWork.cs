using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Infrastructure.EF.Repositories;

namespace ICMarkets.Infrastructure.EF;

public class UnitOfWork : IUnitOfWork
{
    private readonly BlockchainDbContext _db;

    public UnitOfWork(BlockchainDbContext db)
    {
        _db = db;

        Coins = new CoinRepository(db);
        Chains = new ChainRepository(db);
        CoinChains = new CoinChainRepository(db);
        CoinChainInfos = new CoinChainInfoRepository(db);
    }

    public ICoinRepository Coins { get; }
    public IChainRepository Chains { get; }
    public ICoinChainRepository CoinChains { get; }
    public ICoinChainInfoRepository CoinChainInfos { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
