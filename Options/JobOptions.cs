namespace EarthquakeTracker.Options;

public class JobOptions  // appsetting.json a uygun sectionname, Ayar taşır
{
	public const string SectionName = "Jobs";

	//  recurring job: her dakika
	public string FetchEarthquakesCron { get; set; } = "* * * * *";

	// Eski deprem temizliği: her gece 03:00 (Türkiye saati)
	public string CleanupCron { get; set; } = "0 3 * * *";

	// Bu günden eski depremler silinir
	public int RetentionDays { get; set; } = 120;
}
