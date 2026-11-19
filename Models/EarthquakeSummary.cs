namespace EarthquakeTracker.Models;

// Depremler sayfasının üstündeki özet kartlarının verisi
public class EarthquakeSummary
{
	public int Last24HoursCount { get; set; }
	public double? Last24HoursMaxMagnitude { get; set; }
	public string? Last24HoursMaxPlace { get; set; }
	public int UnreadAlertCount { get; set; }
	public int TotalCount { get; set; }
}