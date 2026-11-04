using EarthquakeTracker.Helpers;
using EarthquakeTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace EarthquakeTracker.Controllers;

public class AlertsController : Controller
{
	private readonly AlertService _alertService;
	public AlertsController(AlertService alertService)
	{
		_alertService = alertService;
	}

	// GET /Alerts?onlyUnread=true&page=2
	public async Task<IActionResult> Index(bool onlyUnread = false, int page = 1, CancellationToken cancellationToken = default)
	{
		var result = await _alertService.GetAlertsAsync(onlyUnread, page, cancellationToken);

		ViewData["OnlyUnread"] = onlyUnread;

		return View(result);
	}

	// POST /Alerts/MarkAsRead/5
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> MarkAsRead(int id, string? returnUrl, CancellationToken cancellationToken)
	{
		var found = await _alertService.MarkAsReadAsync(id, cancellationToken);

		if (!found)
		{
			return NotFound();
		}

		return RedirectBack(returnUrl);
	}

	// POST /Alerts/MarkAllAsRead
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> MarkAllAsRead(string? returnUrl, CancellationToken cancellationToken)
	{
		var updatedCount = await _alertService.MarkAllAsReadAsync(cancellationToken);

		TempData[TempDataKeys.Success] = $"{updatedCount} uyarı okundu olarak işaretlendi.";

		return RedirectBack(returnUrl);
	}

	// İşlemden sonra kullanıcıyı geldiği sayfaya (filtre ve sayfa numarasıyla) geri gönderir
	private IActionResult RedirectBack(string? returnUrl)
	{
		if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
		{
			return LocalRedirect(returnUrl);
		}

		return RedirectToAction(nameof(Index));
	}
}