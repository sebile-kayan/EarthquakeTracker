using EarthquakeTracker.Options;
using EarthquakeTracker.Services;
using Hangfire;
using Microsoft.Extensions.Options;

namespace EarthquakeTracker.Jobs;

// Eski depremleri temizleyen gece job'ı. İşi EarthquakeService yapar.
public class CleanupOldEarthquakesJob
{
	private readonly EarthquakeService _earthquakeService;
	private readonly JobOptions _options;
	private readonly ILogger<CleanupOldEarthquakesJob> _logger;

	public CleanupOldEarthquakesJob(
		EarthquakeService earthquakeService,
		IOptions<JobOptions> options,
		ILogger<CleanupOldEarthquakesJob> logger)
	{
		_earthquakeService = earthquakeService;
		_options = options.Value;
		_logger = logger;
	}

	[DisableConcurrentExecution(timeoutInSeconds: 300)]
	public async Task<int> RunAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Temizlik job'ı başladı.");

		var deletedCount = await _earthquakeService.DeleteOlderThanAsync(_options.RetentionDays, cancellationToken);

		_logger.LogInformation("Temizlik job'ı bitti. {DeletedCount} kayıt silindi.", deletedCount);

		return deletedCount;
	}
}
