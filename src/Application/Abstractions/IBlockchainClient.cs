using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Queries.Response;
using ICMarkets.Domain.Entities;

namespace ICMarkets.Application.Abstractions;

/// <summary>Gets blockchain data from an external provider. Implemented in Infrastructure.</summary>
public interface IBlockchainClient
{
    /// <summary>Current state of the chain as a snapshot of the pair's family, ready to be saved.</summary>
    Task<CoinChainInfo> GetCoinChainInfoAsync(CoinChain coinChain, CancellationToken ct = default);
}
