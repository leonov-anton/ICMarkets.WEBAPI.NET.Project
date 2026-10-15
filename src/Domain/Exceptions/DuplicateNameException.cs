using System;

namespace ICMarkets.Domain.Exceptions;

public class DuplicateNameException : InvalidOperationException
{
    public Type EntityType { get; }
    public string Name { get; }

    public DuplicateNameException(Type entityType, string name)
        : base($"{entityType.Name} with Name '{name}' already exists.")
    {
        EntityType = entityType;
        Name = name;
    }
}
