using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICMarkets.API;
using ICMarkets.Domain.Entities;
using ICMarkets.Infrastructure.EF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ICMarkets.IntegrationTests;

/// <summary>
/// Runs the API against throwaway PostgreSQL (with migrations and their seed data applied) and Redis containers.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build();
    private readonly RedisContainer _redis = new RedisBuilder().WithImage("redis:7-alpine").Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_db.StartAsync(), _redis.StartAsync());

        // Before the host starts: ReferenceDataLoader reads coins and chains on startup.
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
        await _redis.DisposeAsync();
    }

    public BlockchainDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<BlockchainDbContext>()
            .UseNpgsql(_db.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options);

    /// <summary>Replaces all snapshots with the given ones; CreatedAt is set explicitly to control ordering.</summary>
    public async Task SeedSnapshotsAsync(params (CoinChainInfo Info, DateTime CreatedAt)[] snapshots)
    {
        await using var context = CreateDbContext();
        await context.CoinChainInfos.ExecuteDeleteAsync();

        foreach (var (info, createdAt) in snapshots)
        {
            context.CoinChainInfos.Add(info);
            context.Entry(info).Property(x => x.CreatedAt).CurrentValue = createdAt;
        }

        await context.SaveChangesAsync();
    }

    public CoinChain GetCoinChain(string coinShortName, string chainName)
    {
        using var context = CreateDbContext();
        return context.CoinChains
            .Include(x => x.Coin)
            .Include(x => x.Chain)
            .Single(x => x.Coin.ShortName == coinShortName && x.Chain.Name == chainName);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Added after appsettings*.json, so these values win.
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:Blockchain"] = _db.GetConnectionString(),
            ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
            ["dataProvider:periodicDataRequestEnabled"] = "false",
        }));
    }
}
