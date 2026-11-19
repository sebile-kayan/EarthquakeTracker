namespace EarthquakeTracker.Helpers;

public static class MagnitudeHelper
{
	// Büyüklüğe göre rozet rengi: <4 yeşil, 4-5 sarı, 5-6 turuncu, 6+ kırmızı
	public static string BadgeClass(double magnitude) => magnitude switch
	{
		< 4.0 => "mag-badge mag-low",
		< 5.0 => "mag-badge mag-moderate",
		< 6.0 => "mag-badge mag-strong",
		_ => "mag-badge mag-major"
	};
}