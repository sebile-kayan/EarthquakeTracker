using EarthquakeTracker.Entities;
using EarthquakeTracker.Exceptions;
using EarthquakeTracker.Helpers;
using EarthquakeTracker.Models.Common;
using EarthquakeTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace EarthquakeTracker.Controllers;

public class EarthquakesController : Controller
{
	private readonly UsgsClient _usgsClient;
	private readonly EarthquakeService _earthquakeService;

	public EarthquakesController(UsgsClient usgsClient, EarthquakeService earthquakeService)
	{
		_usgsClient = usgsClient;
		_earthquakeService = earthquakeService;

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
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Sync(CancellationToken cancellationToken)
	{
		try
		{
			var newCount = await _earthquakeService.SaveNewEarthquakesAsync(cancellationToken);
			TempData[TempDataKeys.Success] = $"{newCount} yeni deprem kaydedildi.";
		}
		catch (ExternalServiceException ex)
		{
			TempData[TempDataKeys.Error] = ex.Message;
		}

		return RedirectToAction(nameof(Index));
	}
}