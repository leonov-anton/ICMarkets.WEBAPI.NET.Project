namespace ICMarkets.Domain.Entities;

public class Chain
{
    public int Id { get; }
    public string Name { get; }

    private Chain()
    {
    }

    public Chain(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;
}
