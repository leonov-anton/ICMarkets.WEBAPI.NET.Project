using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICMarkets.Infrastructure.EF.Configurations;

public class CoinConfiguration : IEntityTypeConfiguration<Coin>
{
    public void Configure(EntityTypeBuilder<Coin> builder)
    {
        builder.ToTable("coins");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ShortName).HasMaxLength(16).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.ShortName).IsUnique();

        // Initial data only; later rows can be added straight to the table (with an explicit id).
        builder.HasData(
            new { Id = 1, Name = "Bitcoin", ShortName = "BTC" },
            new { Id = 2, Name = "Dash", ShortName = "DASH" },
            new { Id = 3, Name = "Dogecoin", ShortName = "DOGE" },
            new { Id = 4, Name = "Litecoin", ShortName = "LTC" },
            new { Id = 5, Name = "BlockCypher", ShortName = "BCY" },
            new { Id = 6, Name = "Ethereum", ShortName = "ETH" });
    }
}
