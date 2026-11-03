namespace EarthquakeTracker.Models.Common;

public static class PagingDefaults
{
	public const int PageSize = 2;
}

// Partial view'ın T'yi bilmeden kullanabileceği ortak bilgiler
public interface IPagedResult
{
	int Page { get; }
	int PageSize { get; }
	int TotalCount { get; }
	int TotalPages { get; }
	bool HasPreviousPage { get; }
	bool HasNextPage { get; }
}

public class PagedResult<T> : IPagedResult
{
	public List<T> Items { get; }
	public int Page { get; }
	public int PageSize { get; }
	public int TotalCount { get; }

	public PagedResult(List<T> items, int page, int pageSize, int totalCount)
	{
		Items = items;
		Page = page;
		PageSize = pageSize;
		TotalCount = totalCount;
	}

	public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
	public bool HasPreviousPage => Page > 1;
	public bool HasNextPage => Page < TotalPages;

	public static PagedResult<T> Empty() => new(new List<T>(), 1, PagingDefaults.PageSize, 0);
}