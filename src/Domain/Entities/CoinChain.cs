using ICMarkets.Domain.Enums;

namespace ICMarkets.Domain.Entities;

public class CoinChain
{
    public int Id { get; }
    public Coin Coin { get; }
    public Chain Chain { get; }
    public ChainFamily Family { get; }

    private CoinChain()
    {
    }

    public CoinChain(int id, Coin coin, Chain chain, ChainFamily family)
    {
        Id = id;
        Coin = coin;
        Chain = chain;
        Family = family;
    }

    public override string ToString() => $"{Coin?.ShortName}.{Chain?.Name}";
}
