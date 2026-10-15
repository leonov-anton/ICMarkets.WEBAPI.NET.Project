using System;
using System.Text.Json.Serialization;

namespace ICMarkets.Application.Queries.Response;

// Without these attributes System.Text.Json would serialize only the base properties.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "family")]
[JsonDerivedType(typeof(UtxoCoinChainInfoResponse), "utxo")]
[JsonDerivedType(typeof(EvmCoinChainInfoResponse), "evm")]
public abstract record CoinChainInfoResponse
{
    public string Name { get; init; }
    public long Height { get; init; }
    public string Hash { get; init; }
    public DateTime Time { get; init; }
    public string PreviousHash { get; init; }
    public int PeerCount { get; init; }
    public int UnconfirmedCount { get; init; }
    public long LastForkHeight { get; init; }
    public string LastForkHash { get; init; }
    public DateTime CreatedAt { get; init; }
}
