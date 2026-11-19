using EarthquakeTracker.Entities;
using EarthquakeTracker.Exceptions;
using EarthquakeTracker.Helpers;
using EarthquakeTracker.Jobs;
using EarthquakeTracker.Models.Common;
using EarthquakeTracker.Services;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace EarthquakeTracker.Controllers;

public class EarthquakesController : Controller
{
	private readonly UsgsClient _usgsClient;
	private readonly EarthquakeService _earthquakeService;
	private readonly IBackgroundJobClient _backgroundJobClient;

	public EarthquakesController(
		UsgsClient usgsClient,
		EarthquakeService earthquakeService,
		IBackgroundJobClient backgroundJobClient)
	{
		_usgsClient = usgsClient;
		_earthquakeService = earthquakeService;
		_backgroundJobClient = backgroundJobClient;
	}

	// GET /Earthquakes?minMagnitude=4&place=Feke&page=2    DB'deki önceden kaydedilmiş kayıtları getirir USGS'ye gitmez
	public async Task<IActionResult> Index(double? minMagnitude, string? place, int page = 1, CancellationToken cancellationToken = default)
	{
		var model = await _earthquakeService.GetEarthquakesAsync(minMagnitude, place, page, cancellationToken);
		return View(model);
	}

	// GET /Earthquakes/Details/5    DB'deki depremlerin detayları
	public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
	{
		var earthquake = await _earthquakeService.GetByIdAsync(id, cancellationToken);

		if (earthquake == null)
		{
			return NotFound();
		}

		return View(earthquake);
	}

	// GET /Earthquakes/Live
	// Direkt usgs'deki verileri gösterir. Usgsclient servisiyle deprem verilerini çekiyoruz daha sonra çekilen verileri usgsmapper ile bizim kullanabileceğimiz hale getiriyoruz.
	// GET /Earthquakes/Live?page=2
	public async Task<IActionResult> Live(int page = 1, CancellationToken cancellationToken = default)
	{
		try
		{
			var features = await _usgsClient.FetchLatestAsync(cancellationToken);

			var result = features
				.Select(UsgsMapper.ToEarthquake)
				.ToPagedResult(page);

			return View(result);
		}
		catch (ExternalServiceException ex)
		{
			TempData[TempDataKeys.Error] = ex.Message;
			return View(PagedResult<Earthquake>.Empty());
		}
	}

	// POST/Earthquakes/Sync    Yeni deprem ise depremi usgsden çeker ve USGS’den alıp DB’ye yazabilmemizi sağlar
	// Deprem çekme işini Hangfire kuyruğuna atar ve hemen döner. Şimdi güncelle butonu ile tetikleniyor.
	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult Sync()
	{
		var jobId = _backgroundJobClient.Enqueue<FetchEarthquakesJob>(
		job => job.RunAsync(null, CancellationToken.None));

		TempData[TempDataKeys.Success] =
			$"Güncelleme başlatıldı (Job #{jobId}). Birkaç saniye sonra sayfayı yenileyin.";

		return RedirectToAction(nameof(Index));
	}
}