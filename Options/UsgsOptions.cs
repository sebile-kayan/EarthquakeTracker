namespace EarthquakeTracker.Options;

public class UsgsOptions  // appsetting.json a uygun sectionname, Ayar taşır
{
	public const string SectionName = "Usgs";

	public string BaseUrl { get; set; } = "https://earthquake.usgs.gov/";
	public int TimeoutSeconds { get; set; } = 15;
	public double MinMagnitude { get; set; } = 2.5;
	public int LookbackHours { get; set; } = 24;

	// null ise dünya geneli
	public UsgsRegion? Region { get; set; }
}

public class UsgsRegion
{
	public double MinLatitude { get; set; }
	public double MaxLatitude { get; set; }
	public double MinLongitude { get; set; }
	public double MaxLongitude { get; set; }
}