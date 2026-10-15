using ICMarkets.Domain.Entities;
using ICMarkets.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICMarkets.Infrastructure.EF.Configurations;

public class CoinChainConfiguration : IEntityTypeConfiguration<CoinChain>
{
    private const string CoinIdColumn = "CoinId";
    private const string ChainIdColumn = "ChainId";

    public void Configure(EntityTypeBuilder<CoinChain> builder)
    {
        builder.ToTable("coin_chains");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Family).IsRequired();

        // Foreign keys are shadow properties (the domain only holds the navigations);
        // shadow keys default to nullable, hence IsRequired.
        builder.HasOne(x => x.Coin).WithMany().HasForeignKey(CoinIdColumn).IsRequired().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Chain).WithMany().HasForeignKey(ChainIdColumn).IsRequired().OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(CoinIdColumn, ChainIdColumn).IsUnique();

        // Initial data only; later rows can be added straight to the table (with an explicit id).
        builder.HasData(
            new { Id = 1, CoinId = 1, ChainId = 1, Family = ChainFamily.Utxo },
            new { Id = 2, CoinId = 1, ChainId = 2, Family = ChainFamily.Utxo },
            new { Id = 3, CoinId = 2, ChainId = 1, Family = ChainFamily.Utxo },
            new { Id = 4, CoinId = 3, ChainId = 1, Family = ChainFamily.Utxo },
            new { Id = 5, CoinId = 4, ChainId = 1, Family = ChainFamily.Utxo },
            new { Id = 6, CoinId = 5, ChainId = 3, Family = ChainFamily.Utxo },
            new { Id = 7, CoinId = 6, ChainId = 1, Family = ChainFamily.Evm });
    }
}
