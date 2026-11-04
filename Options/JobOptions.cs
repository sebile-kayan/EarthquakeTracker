namespace EarthquakeTracker.Options;

public class JobOptions  // appsetting.json a uygun sectionname
{
	public const string SectionName = "Jobs";

	//  recurring job: her dakika
	public string FetchEarthquakesCron { get; set; } = "* * * * *";
}