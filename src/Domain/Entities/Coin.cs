namespace ICMarkets.Domain.Entities;

public class Coin
{
    public int Id { get; }
    public string Name { get; }
    public string ShortName { get; }

    private Coin()
    {
    }

    public Coin(int id, string name, string shortName)
    {
        Id = id;
        Name = name;
        ShortName = shortName;
    }

    public override string ToString() => Name;
}
