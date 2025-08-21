namespace Million.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public long Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

