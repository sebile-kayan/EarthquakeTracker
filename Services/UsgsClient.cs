using EarthquakeTracker.Exceptions;
using EarthquakeTracker.Models.Usgs;
using System.Text.Json;

namespace EarthquakeTracker.Services;

public class UsgsClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<UsgsClient> _logger;

	public UsgsClient(HttpClient httpClient, ILogger<UsgsClient> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}

	public async Task<List<UsgsFeature>> FetchLatestAsync(CancellationToken cancellationToken = default)
	{
		var url = "fdsnws/event/1/query?format=geojson"
				+ "&minlatitude=35&maxlatitude=43"
				+ "&minlongitude=25&maxlongitude=45"
				+ "&minmagnitude=2.5"
				+ "&orderby=time";

		try
		{
			var response = await _httpClient.GetFromJsonAsync<UsgsResponse>(url, cancellationToken);
			return response?.Features ?? new List<UsgsFeature>();
		}
		catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)   // Kullanıcının sayfayı kendi kapattığı durumlar dışındaki hataları yakalar
		{
			// İptal isteyen kullanıcı değilse, timeout dolmuştur
			_logger.LogWarning(ex, "USGS {TimeoutSeconds} saniye içinde cevap vermedi.",
				_httpClient.Timeout.TotalSeconds);

			throw new ExternalServiceException(
				"USGS zamanında cevap vermedi. Lütfen biraz sonra tekrar deneyin.", ex);
		}
		catch (HttpRequestException ex)
		{
			_logger.LogWarning(ex, "USGS isteği başarısız oldu. HTTP durum kodu: {StatusCode}",
				ex.StatusCode);

			throw new ExternalServiceException(
				"USGS servisine şu an ulaşılamıyor. Lütfen biraz sonra tekrar deneyin.", ex);
		}
		catch (JsonException ex)
		{
			_logger.LogError(ex, "USGS cevabı beklenen formatta değil.");

			throw new ExternalServiceException(
				"USGS'den beklenmeyen formatta veri geldi.", ex);
		}
	}
}