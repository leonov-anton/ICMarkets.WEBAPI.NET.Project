using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICMarkets.Infrastructure.EF.Configurations;

public class ChainConfiguration : IEntityTypeConfiguration<Chain>
{
    public void Configure(EntityTypeBuilder<Chain> builder)
    {
        builder.ToTable("chains");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        // Initial data only; later rows can be added straight to the table (with an explicit id).
        builder.HasData(
            new { Id = 1, Name = "Main" },
            new { Id = 2, Name = "Testnet3" },
            new { Id = 3, Name = "Test" });
    }
}
