using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICMarkets.Infrastructure.EF.Configurations;

// Stored in the coin_chain_infos table (TPH, see CoinChainInfoConfiguration); columns are NULL for other families.
public class UtxoCoinChainInfoConfiguration : IEntityTypeConfiguration<UtxoCoinChainInfo>
{
    public void Configure(EntityTypeBuilder<UtxoCoinChainInfo> builder)
    {
        builder.Property(x => x.HighFeePerKb);
        builder.Property(x => x.MediumFeePerKb);
        builder.Property(x => x.LowFeePerKb);
    }
}
