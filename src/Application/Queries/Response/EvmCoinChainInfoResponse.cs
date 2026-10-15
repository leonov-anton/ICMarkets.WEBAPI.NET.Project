namespace ICMarkets.Application.Queries.Response;

public record EvmCoinChainInfoResponse : CoinChainInfoResponse
{
    public long HighGasPrice { get; init; }
    public long MediumGasPrice { get; init; }
    public long LowGasPrice { get; init; }
    public long HighPriorityFee { get; init; }
    public long MediumPriorityFee { get; init; }
    public long LowPriorityFee { get; init; }
    public long BaseFee { get; init; }
}
