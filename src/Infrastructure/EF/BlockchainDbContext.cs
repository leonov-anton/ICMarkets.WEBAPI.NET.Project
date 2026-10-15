using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.EF;

public class BlockchainDbContext : DbContext
{
    public DbSet<Coin> Coins => Set<Coin>();
    public DbSet<Chain> Chains => Set<Chain>();
    public DbSet<CoinChain> CoinChains => Set<CoinChain>();
    public DbSet<CoinChainInfo> CoinChainInfos => Set<CoinChainInfo>();

    public BlockchainDbContext(DbContextOptions<BlockchainDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlockchainDbContext).Assembly);
    }
}
