using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ICMarkets.Domain.Exceptions;

namespace ICMarkets.Domain.Enums;

/// <summary>
/// Base class for enumeration-like classes (Coin, Chain).
/// </summary>
public abstract class Enumeration<T, TId>
    where T : Enumeration<T, TId>
    where TId : IEquatable<TId>
{
    private static readonly Dictionary<TId, T> IdInstances = new Dictionary<TId, T>();
    private static readonly Dictionary<string, T> NameInstances = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

    // Static members of the base class do not trigger the derived type's static initializers,
    // so force them to run; otherwise the dictionaries stay empty until the derived type is touched.
    static Enumeration()
    {
        RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
    }

    public TId Id { get; }
    public string Name { get; }

    protected Enumeration(TId id, string name)
    {
        if (IdInstances.ContainsKey(id))
            throw new DuplicateIdException(typeof(T), id);
        if (NameInstances.ContainsKey(name))
            throw new DuplicateNameException(typeof(T), name);

        Id = id;
        Name = name;

        IdInstances.Add(id, (T)this);
        NameInstances.Add(name, (T)this);
    }

    public static IReadOnlyList<T> GetAll() => IdInstances.Values.ToList();

    public static bool TryGetByName(string name, out T item)
    {
        item = null;
        return name != null && NameInstances.TryGetValue(name, out item);
    }

    public static bool ExistsById(TId id) => IdInstances.ContainsKey(id);

    public static bool ExistsByName(string name) => NameInstances.ContainsKey(name);

    public static bool Exists(TId id, string name) =>
        IdInstances.TryGetValue(id, out var item)
        && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Name;
}
