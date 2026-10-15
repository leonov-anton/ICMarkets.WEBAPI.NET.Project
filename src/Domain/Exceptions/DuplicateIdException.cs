using System;

namespace ICMarkets.Domain.Exceptions;

public class DuplicateIdException : InvalidOperationException
{
    public Type EntityType { get; }
    public object Id { get; }

    public DuplicateIdException(Type entityType, object id)
        : base($"{entityType.Name} with Id '{id}' already exists.")
    {
        EntityType = entityType;
        Id = id;
    }
}
