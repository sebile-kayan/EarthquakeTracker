using EarthquakeTracker.Entities;
using EarthquakeTracker.Models.Common;

namespace EarthquakeTracker.Models;

public class EarthquakeListViewModel
{
	public PagedResult<Earthquake> Result { get; set; } = PagedResult<Earthquake>.Empty();

	public double? MinMagnitude { get; set; }
	public string? Place { get; set; }
}