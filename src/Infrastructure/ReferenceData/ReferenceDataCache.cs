using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain.Entities;

namespace ICMarkets.Infrastructure.ReferenceData;

public class ReferenceDataCache : IReferenceDataCache
{
    private volatile Snapshot _snapshot;

    public IReadOnlyList<Coin> Coins => Current.Coins;
    public IReadOnlyList<Chain> Chains => Current.Chains;
    public IReadOnlyList<CoinChain> CoinChains => Current.CoinChains;

    private Snapshot Current =>
        _snapshot ?? throw new InvalidOperationException("Reference data has not been loaded yet.");

    public bool TryGetCoinChain(int id, out CoinChain coinChain) =>
        Current.ById.TryGetValue(id, out coinChain);

    public bool TryGetCoinChain(string coinShortName, string chainName, out CoinChain coinChain)
    {
        coinChain = null;
        return coinShortName != null
            && chainName != null
            && Current.ByNames.TryGetValue(Snapshot.NameKey(coinShortName, chainName), out coinChain);
    }

    public async Task LoadAsync(IUnitOfWork unitOfWork, CancellationToken ct = default)
    {
        var coins = await unitOfWork.Coins.GetAllAsync(ct);
        var chains = await unitOfWork.Chains.GetAllAsync(ct);
        var coinChains = await unitOfWork.CoinChains.GetAllAsync(ct);

        _snapshot = new Snapshot(coins, chains, coinChains);
    }

    private sealed class Snapshot
    {
        public IReadOnlyList<Coin> Coins { get; }
        public IReadOnlyList<Chain> Chains { get; }
        public IReadOnlyList<CoinChain> CoinChains { get; }
        public IReadOnlyDictionary<int, CoinChain> ById { get; }
        public IReadOnlyDictionary<string, CoinChain> ByNames { get; }

        public Snapshot(IReadOnlyList<Coin> coins, IReadOnlyList<Chain> chains, IReadOnlyList<CoinChain> coinChains)
        {
            Coins = coins;
            Chains = chains;
            CoinChains = coinChains;
            ById = coinChains.ToDictionary(x => x.Id);
            ByNames = coinChains.ToDictionary(
                x => NameKey(x.Coin.ShortName, x.Chain.Name),
                StringComparer.OrdinalIgnoreCase);
        }

        public static string NameKey(string coinShortName, string chainName) => $"{coinShortName}.{chainName}";
    }
}
