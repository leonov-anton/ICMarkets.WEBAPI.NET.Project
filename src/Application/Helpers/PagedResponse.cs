using System;
using System.Collections.Generic;
using System.Linq;

namespace ICMarkets.Application.Helpers;

public record PagedResponse<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public static PagedResponse<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Max(pageSize, 1);

        var all = source as IReadOnlyCollection<T> ?? source.ToList();
        var items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResponse<T>(items, pageNumber, pageSize, all.Count);
    }
}
