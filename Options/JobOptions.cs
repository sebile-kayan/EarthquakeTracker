namespace EarthquakeTracker.Options;

public class JobOptions  // appsetting.json a uygun sectionname, Ayar taşır
{
	public const string SectionName = "Jobs";

	//  recurring job: her dakika
	public string FetchEarthquakesCron { get; set; } = "* * * * *";
}