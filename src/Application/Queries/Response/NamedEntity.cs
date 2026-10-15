namespace ICMarkets.Application.Queries.Response;

public record struct NamedEntity<TId>(TId Id, string Name);
