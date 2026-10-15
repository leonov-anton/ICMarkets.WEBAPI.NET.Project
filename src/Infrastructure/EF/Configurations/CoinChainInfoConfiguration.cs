using ICMarkets.Domain.Entities;
using ICMarkets.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICMarkets.Infrastructure.EF.Configurations;

// All snapshots live in one table (TPH): no joins on read, one index for history queries.
// Columns of other families stay NULL, which costs only a bit in the PostgreSQL null bitmap.
// Domain properties are get-only, so every column is mapped explicitly (EF writes the backing fields).

public class CoinChainInfoConfiguration : IEntityTypeConfiguration<CoinChainInfo>
{
    private const int HashMaxLength = 128;

    public void Configure(EntityTypeBuilder<CoinChainInfo> builder)
    {
        builder.ToTable("coin_chain_infos");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();

        builder.HasDiscriminator<ChainFamily>("Family")
            .HasValue<UtxoCoinChainInfo>(ChainFamily.Utxo)
            .HasValue<EvmCoinChainInfo>(ChainFamily.Evm);

        builder.Property(x => x.CoinChainId).IsRequired();
        builder.Property(x => x.Height).IsRequired();
        builder.Property(x => x.Hash).HasMaxLength(HashMaxLength).IsRequired();
        builder.Property(x => x.Time).IsRequired();
        builder.Property(x => x.PreviousHash).HasMaxLength(HashMaxLength);
        builder.Property(x => x.PeerCount).IsRequired();
        builder.Property(x => x.UnconfirmedCount).IsRequired();
        builder.Property(x => x.LastForkHeight).IsRequired();
        builder.Property(x => x.LastForkHash).HasMaxLength(HashMaxLength);
        builder.Property(x => x.CreatedAt).IsRequired();

        // No navigation on purpose: the pair is resolved by CoinChainId through IReferenceDataCache,
        // so loading snapshots never creates CoinChain copies next to the cached instances.
        builder.HasOne<CoinChain>()
            .WithMany()
            .HasForeignKey(x => x.CoinChainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CoinChainId, x.CreatedAt })
            .IsDescending(false, true);
    }
}
