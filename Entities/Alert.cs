namespace EarthquakeTracker.Entities;

public class Alert
{
	public int Id { get; set; }
	public int EarthquakeId { get; set; }
	public Earthquake Earthquake { get; set; } = null!;
	public string Message { get; set; } = string.Empty;
	public bool IsRead { get; set; }
	public DateTime CreatedAtUtc { get; set; }
}