namespace EarthquakeTracker.Options;

public class AlertOptions  // appsetting.json a uygun sectionname
{
	public const string SectionName = "Alerts";
	public double MinMagnitude { get; set; } = 4.5;
}