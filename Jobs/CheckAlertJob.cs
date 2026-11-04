using EarthquakeTracker.Services;

namespace EarthquakeTracker.Jobs;

// Deprem çekme job'ı bitince çalışır.
// Eşiğin üstünde olup uyarısı olmayan depremler için uyarı açar.
public class CheckAlertsJob
{
	private readonly AlertService _alertService;
	private readonly ILogger<CheckAlertsJob> _logger;

	public CheckAlertsJob(AlertService alertService, ILogger<CheckAlertsJob> logger)
	{
		_alertService = alertService;
		_logger = logger;
	}

	public async Task<int> RunAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Uyarı kontrol job'ı başladı.");

		var newAlerts = await _alertService.CreateMissingAlertsAsync(cancellationToken);

		_logger.LogInformation("Uyarı kontrol job'ı bitti. {AlertCount} yeni uyarı oluşturuldu.", newAlerts.Count);

		return newAlerts.Count;
	}
}