using EarthquakeTracker.Entities;
using EarthquakeTracker.Models.Usgs;

namespace EarthquakeTracker.Services;

public static class UsgsMapper
{
	public static Earthquake ToEarthquake(UsgsFeature feature)  //usgsden gelenlerle responseda oluşturduklarımızla earthquakes entitysindekileri mapliyor
	{
		var coordinates = feature.Geometry.Coordinates;

		return new Earthquake
		{
			ExternalId = feature.Id,
			Magnitude = feature.Properties.Magnitude ?? 0,
			Place = feature.Properties.Place ?? "Bilinmeyen konum",

			// GeoJSON sırası: [boylam, enlem, derinlik]
			Longitude = coordinates.Length > 0 ? coordinates[0] : 0,
			Latitude = coordinates.Length > 1 ? coordinates[1] : 0,
			DepthKm = coordinates.Length > 2 ? coordinates[2] : 0,

			OccurredAtUtc = DateTimeOffset
				.FromUnixTimeMilliseconds(feature.Properties.TimeUnixMs)
				.UtcDateTime,

			CreatedAtUtc = DateTime.UtcNow
		};
	}
}