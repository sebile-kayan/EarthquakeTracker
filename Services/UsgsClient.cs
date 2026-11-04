using EarthquakeTracker.Exceptions;
using EarthquakeTracker.Models.Usgs;
using EarthquakeTracker.Options;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace EarthquakeTracker.Services;

public class UsgsClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<UsgsClient> _logger;
	private readonly UsgsOptions _options;

	public UsgsClient(HttpClient httpClient, ILogger<UsgsClient> logger, IOptions<UsgsOptions> options)
	{
		_httpClient = httpClient;
		_logger = logger;
		_options = options.Value;
	}

	public async Task<List<UsgsFeature>> FetchLatestAsync(CancellationToken cancellationToken = default)
	{
		var url = BuildQueryUrl();

		try
		{
			var response = await _httpClient.GetFromJsonAsync<UsgsResponse>(url, cancellationToken);
			return response?.Features ?? new List<UsgsFeature>();
		}
		catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
		{
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

	private string BuildQueryUrl()
	{
		var startTime = DateTime.UtcNow
			.AddHours(-_options.LookbackHours)
			.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

		var parameters = new List<string>
		{
			"format=geojson",
			"orderby=time",
			$"starttime={startTime}",
			$"minmagnitude={Format(_options.MinMagnitude)}"
		};

		// Bölge verilmişse kutuyla sınırla, verilmemişse dünya geneli
		if (_options.Region is { } region)
		{
			parameters.Add($"minlatitude={Format(region.MinLatitude)}");
			parameters.Add($"maxlatitude={Format(region.MaxLatitude)}");
			parameters.Add($"minlongitude={Format(region.MinLongitude)}");
			parameters.Add($"maxlongitude={Format(region.MaxLongitude)}");
		}

		return "fdsnws/event/1/query?" + string.Join("&", parameters);
	}

	// Adreste ondalık ayırıcı her zaman nokta olmalı: 2.5, 2,5 değil
	private static string Format(double value) =>
		value.ToString(CultureInfo.InvariantCulture);
}