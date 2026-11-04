using EarthquakeTracker.Data;
using EarthquakeTracker.Entities;
using EarthquakeTracker.Helpers;
using EarthquakeTracker.Models.Common;
using EarthquakeTracker.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EarthquakeTracker.Services;

public class AlertService
{
	private const int MaxMessageLength = 250;

	private readonly AppDbContext _db;
	private readonly AlertOptions _options;
	private readonly ILogger<AlertService> _logger;

	public AlertService(AppDbContext db, IOptions<AlertOptions> options, ILogger<AlertService> logger)
	{
		_db = db;
		_options = options.Value;
		_logger = logger;
	}

	// Eşiğin üstünde olup henüz uyarısı olmayan bütün depremler için uyarı açar.
	// Oluşturulan uyarıları geri döner (bildirim göndermek için 12.5'te kullanılacak).
	public async Task<List<Alert>> CreateMissingAlertsAsync(CancellationToken cancellationToken = default)
	{
		// 1. Uyarısı olmayan güçlü depremleri bul
		var earthquakesWithoutAlert = await _db.Earthquakes
			.Where(e => e.Magnitude >= _options.MinMagnitude && e.Alert == null)
			.OrderBy(e => e.OccurredAtUtc)
			.ToListAsync(cancellationToken);

		if (earthquakesWithoutAlert.Count == 0)
		{
			return new List<Alert>();
		}

		// 2. Her biri için bir uyarı oluştur
		var now = DateTime.UtcNow;

		var newAlerts = earthquakesWithoutAlert
			.Select(earthquake => new Alert
			{
				Earthquake = earthquake,
				Message = BuildMessage(earthquake),
				IsRead = false,
				CreatedAtUtc = now
			})
			.ToList();

		// 3. Kaydet
		_db.Alerts.AddRange(newAlerts);
		await _db.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("{AlertCount} yeni uyarı oluşturuldu.", newAlerts.Count);

		return newAlerts;
	}


	// Uyarıları listeler: okunmamışlar üstte, sonra en yeni deprem üstte
	public async Task<PagedResult<Alert>> GetAlertsAsync(bool onlyUnread, int page, CancellationToken cancellationToken = default)
	{
		IQueryable<Alert> query = _db.Alerts
			.AsNoTracking()
			.Include(a => a.Earthquake);

		if (onlyUnread)
		{
			query = query.Where(a => !a.IsRead);
		}

		return await query
			.OrderBy(a => a.IsRead)
			.ThenByDescending(a => a.Earthquake.OccurredAtUtc)
			.ToPagedResultAsync(page, cancellationToken: cancellationToken);
	}

	// Tek bir uyarıyı okundu yapar. Uyarı bulunamazsa false döner.
	public async Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
	{
		var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

		if (alert == null)
		{
			return false;
		}

		if (!alert.IsRead)
		{
			alert.IsRead = true;
			await _db.SaveChangesAsync(cancellationToken);
		}

		return true;
	}

	// Okunmamış bütün uyarıları tek SQL ile okundu yapar. Kaç tane güncellendiğini döner.
	public async Task<int> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
	{
		return await _db.Alerts
			.Where(a => !a.IsRead)
			.ExecuteUpdateAsync(setters => setters.SetProperty(a => a.IsRead, true), cancellationToken);
	}


	private static string BuildMessage(Earthquake earthquake)
	{
		var message = $"{earthquake.Magnitude:0.0} büyüklüğünde deprem: {earthquake.Place}";

		return message.Length > MaxMessageLength
			? message[..MaxMessageLength]
			: message;
	}
}