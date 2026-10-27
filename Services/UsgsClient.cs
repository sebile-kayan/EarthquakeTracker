using EarthquakeTracker.Models.Usgs;

namespace EarthquakeTracker.Services;

public class UsgsClient
{
	private readonly HttpClient _httpClient;

	public UsgsClient(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<List<UsgsFeature>> FetchLatestAsync(CancellationToken cancellationToken = default)
	{
		var url = "fdsnws/event/1/query?format=geojson"
				+ "&minlatitude=35&maxlatitude=43"
				+ "&minlongitude=25&maxlongitude=45"
				+ "&minmagnitude=2.5"
				+ "&orderby=time";

		var response = await _httpClient.GetFromJsonAsync<UsgsResponse>(url, cancellationToken);

		return response?.Features ?? new List<UsgsFeature>();
	}
}