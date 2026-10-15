using System;
using ICMarkets.Domain.Enums;

namespace ICMarkets.Domain.Entities;

/// <summary>
/// Snapshot of a chain state at the moment it was fetched.
/// Fee data depends on the chain family, see <see cref="UtxoCoinChainInfo"/> and <see cref="EvmCoinChainInfo"/>.
/// </summary>
public abstract class CoinChainInfo
{
    public long Id { get; }
    public int CoinChainId { get; }
    public long Height { get; }
    public string Hash { get; }
    public DateTime Time { get; }
    public string PreviousHash { get; }
    public int PeerCount { get; }
    public int UnconfirmedCount { get; }
    public long LastForkHeight { get; }
    public string LastForkHash { get; }
    public DateTime CreatedAt { get; }

    // For EF Core materialization.
    protected CoinChainInfo()
    {
    }

    /// <param name="coinChain">Pair taken from the in-memory reference data.</param>
    /// <param name="family">Family the concrete snapshot type stores fees for.</param>
    protected CoinChainInfo(
        CoinChain coinChain,
        ChainFamily family,
        long height,
        string hash,
        DateTime time,
        string previousHash,
        int peerCount,
        int unconfirmedCount,
        long lastForkHeight,
        string lastForkHash)
    {
        ArgumentNullException.ThrowIfNull(coinChain);

        if (coinChain.Family != family)
            throw new ArgumentException($"Coin chain '{coinChain}' belongs to the {coinChain.Family} family, not {family}.", nameof(coinChain));

        CoinChainId = coinChain.Id;
        Height = height;
        Hash = hash;
        Time = time;
        PreviousHash = previousHash;
        PeerCount = peerCount;
        UnconfirmedCount = unconfirmedCount;
        LastForkHeight = lastForkHeight;
        LastForkHash = lastForkHash;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>True if both snapshots describe the same chain state; Id and CreatedAt are not compared.</summary>
    public virtual bool HasSameDataAs(CoinChainInfo other) =>
        other != null
        && other.GetType() == GetType()
        && CoinChainId == other.CoinChainId
        && Height == other.Height
        && Hash == other.Hash
        && ToMicroseconds(Time) == ToMicroseconds(other.Time)
        && PreviousHash == other.PreviousHash
        && PeerCount == other.PeerCount
        && UnconfirmedCount == other.UnconfirmedCount
        && LastForkHeight == other.LastForkHeight
        && LastForkHash == other.LastForkHash;

    // PostgreSQL keeps microseconds, so a value read back from the database may differ from the original in the last digit.
    private static long ToMicroseconds(DateTime time) => time.Ticks / 10;
}
