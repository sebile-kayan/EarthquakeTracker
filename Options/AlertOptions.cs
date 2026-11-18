namespace EarthquakeTracker.Options;

public class AlertOptions  // appsetting.json a uygun sectionname, Ayar taşır
{
	public const string SectionName = "Alerts";
	public double MinMagnitude { get; set; } = 4.5;
}