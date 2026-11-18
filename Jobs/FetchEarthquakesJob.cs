using EarthquakeTracker.Services;
using Hangfire;
using Hangfire.Server;

namespace EarthquakeTracker.Jobs;

// USGS'den depremleri çekip yenileri kaydeden arka plan job'ı.“Ne zaman” değil “ne yapılacak” sorusunun cevabı. Hangfire: “Ne zaman ve hangi thread’de” sorusunu çözer
// Kim çağırırsa çağırsın (buton, zamanlayıcı) aynı iş yapılır.
// Bitince arkasından CheckAlertsJob çalışır.
public class FetchEarthquakesJob
{
	private readonly EarthquakeService _earthquakeService;
	private readonly IBackgroundJobClient _backgroundJobClient;
	private readonly ILogger<FetchEarthquakesJob> _logger;

	public FetchEarthquakesJob(
		EarthquakeService earthquakeService,
		IBackgroundJobClient backgroundJobClient,
		ILogger<FetchEarthquakesJob> logger)
	{
		_earthquakeService = earthquakeService;
		_backgroundJobClient = backgroundJobClient;
		_logger = logger;
	}

	public async Task<int> RunAsync(PerformContext? context, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Deprem çekme job'ı başladı.");

		var newCount = await _earthquakeService.SaveNewEarthquakesAsync(cancellationToken);

		// Bu job başarıyla bitince uyarı kontrolü çalışsın
		if (context != null)
		{
			_backgroundJobClient.ContinueJobWith<CheckAlertsJob>(
				context.BackgroundJob.Id,
				job => job.RunAsync(CancellationToken.None));
		}

		_logger.LogInformation("Deprem çekme job'ı bitti. {NewCount} yeni deprem kaydedildi.", newCount);

		return newCount;
	}
}