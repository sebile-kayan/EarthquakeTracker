using EarthquakeTracker.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace EarthquakeTracker.Helpers;

public static class PagingExtensions
{
	// DB sorguları için: SQL'den sadece istenen sayfa gelir
	public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize = PagingDefaults.PageSize, CancellationToken cancellationToken = default)
	{
		page = Math.Max(page, 1);

		var totalCount = await query.CountAsync(cancellationToken);

		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return new PagedResult<T>(items, page, pageSize, totalCount);
	}

	// Bellekteki listeler için (örn. USGS'den gelen veri)
	public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int page, int pageSize = PagingDefaults.PageSize)
	{
		page = Math.Max(page, 1);

		var all = source.ToList();

		var items = all
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return new PagedResult<T>(items, page, pageSize, all.Count);
	}
}