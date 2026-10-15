using System.Collections.Generic;
using ICMarkets.Domain.Entities;

namespace ICMarkets.Application.Abstractions;

public interface IReferenceDataCache
{
    IReadOnlyList<Coin> Coins { get; }
    IReadOnlyList<Chain> Chains { get; }
    IReadOnlyList<CoinChain> CoinChains { get; }

    bool TryGetCoinChain(int id, out CoinChain coinChain);

    bool TryGetCoinChain(string coinShortName, string chainName, out CoinChain coinChain);
}
