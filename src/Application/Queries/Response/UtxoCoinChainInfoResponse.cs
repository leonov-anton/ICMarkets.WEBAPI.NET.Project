namespace ICMarkets.Application.Queries.Response;

public record UtxoCoinChainInfoResponse : CoinChainInfoResponse
{
    public long HighFeePerKb { get; init; }
    public long MediumFeePerKb { get; init; }
    public long LowFeePerKb { get; init; }
}
