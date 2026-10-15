using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Infrastructure.BlockCypher.Models.Response;
using RestEase;

namespace ICMarkets.Infrastructure.BlockCypher;

public interface IBlockCypherApi
{
    [Get("v1/{coin}/{chain}")]
    Task<BlockCypherChainInfoResponse> GetChainInfoAsync(
        [Path] string coin,
        [Path] string chain,
        CancellationToken ct = default);
}
