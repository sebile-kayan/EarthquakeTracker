namespace EarthquakeTracker.Helpers;

public static class TimeHelper
{
	private static readonly TimeZoneInfo TurkeyTimeZone =
		TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");

	public static DateTime ToTurkeyTime(this DateTime utcTime)
	{
		return TimeZoneInfo.ConvertTimeFromUtc(
			DateTime.SpecifyKind(utcTime, DateTimeKind.Utc),
			TurkeyTimeZone);
	}
}