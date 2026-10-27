/*
ExternalId Dış kimlik: depremin USGS’deki numarası "us7000n1ab"
Magnitude: Büyüklük		DepthKm: Derinlik (kilometre)	Place: yer adı    
Latitude: Enlem    Longitude: Boylam		Alert: Bu depremin uyarısı
OccurredAtUtc:Olduğu zaman (UTC saatiyle		CreatedAtUtc:Bizim DB’ye kaydedildiği zaman(UTC)
*/

namespace EarthquakeTracker.Entities;

public class Earthquake
{
	public int Id { get; set; }

	public string ExternalId { get; set; } = string.Empty; 

	public double Magnitude { get; set; }
	public double DepthKm { get; set; }
	public string Place { get; set; } = string.Empty;

	public double Latitude { get; set; }
	public double Longitude { get; set; }

	public DateTime OccurredAtUtc { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	public Alert? Alert { get; set; }
}
