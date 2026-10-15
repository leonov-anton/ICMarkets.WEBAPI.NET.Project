namespace ICMarkets.Application.Queries.Response;

/// <summary>Supported pair; Coin and Chain are the values to use in the history route.</summary>
public record CoinChainResponse(int Id, string Coin, string Chain, string Family);
