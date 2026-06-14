namespace Identity.Application.Common;

public sealed class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int totalItems)
    {
        Items = items;
        TotalItems = totalItems;
    }

    public IReadOnlyList<T> Items { get; }

    public int TotalItems { get; }
}
