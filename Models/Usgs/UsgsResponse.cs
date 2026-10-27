using System.Text.Json.Serialization;

namespace EarthquakeTracker.Models.Usgs;

// Cevabın en dış katmanı
public class UsgsResponse
{
	[JsonPropertyName("features")]
	public List<UsgsFeature> Features { get; set; } = new();
}

// Her deprem bir "feature"
public class UsgsFeature
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("properties")]
	public UsgsProperties Properties { get; set; } = new();

	[JsonPropertyName("geometry")]
	public UsgsGeometry Geometry { get; set; } = new();
}

// Depremin bilgileri
public class UsgsProperties
{
	[JsonPropertyName("mag")]
	public double? Magnitude { get; set; }

	[JsonPropertyName("place")]
	public string? Place { get; set; }

	// 1970'ten beri geçen milisaniye (UTC)
	[JsonPropertyName("time")]
	public long TimeUnixMs { get; set; }
}

// Konum: [boylam, enlem, derinlik]
public class UsgsGeometry
{
	[JsonPropertyName("coordinates")]
	public double[] Coordinates { get; set; } = Array.Empty<double>();
}