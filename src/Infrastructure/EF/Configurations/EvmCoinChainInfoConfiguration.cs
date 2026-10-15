using ICMarkets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ICMarkets.Infrastructure.EF.Configurations;

// Stored in the coin_chain_infos table (TPH, see CoinChainInfoConfiguration); columns are NULL for other families.
public class EvmCoinChainInfoConfiguration : IEntityTypeConfiguration<EvmCoinChainInfo>
{
    public void Configure(EntityTypeBuilder<EvmCoinChainInfo> builder)
    {
        builder.Property(x => x.HighGasPrice);
        builder.Property(x => x.MediumGasPrice);
        builder.Property(x => x.LowGasPrice);
        builder.Property(x => x.HighPriorityFee);
        builder.Property(x => x.MediumPriorityFee);
        builder.Property(x => x.LowPriorityFee);
        builder.Property(x => x.BaseFee);
    }
}
