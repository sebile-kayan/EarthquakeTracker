using EarthquakeTracker.Data;
using EarthquakeTracker.Entities;
using EarthquakeTracker.Helpers;
using EarthquakeTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace EarthquakeTracker.Services;

public class EarthquakeService
{
	private readonly AppDbContext _db;
	private readonly UsgsClient _usgsClient;
	private readonly ILogger<EarthquakeService> _logger;

	public EarthquakeService(AppDbContext db, UsgsClient usgsClient, ILogger<EarthquakeService> logger)
	{
		_db = db;
		_usgsClient = usgsClient;
		_logger = logger;
	}

	// USGS'den çeker, sadece yeni olanları kaydeder, kaç tane kaydettiğini döner
	public async Task<int> SaveNewEarthquakesAsync(CancellationToken cancellationToken = default)
	{
		// 1. USGS'den çek
		var features = await _usgsClient.FetchLatestAsync(cancellationToken);

		// 2. Büyüklüğü olmayanları at, kalanları Earthquake'e çevir
		var incomingEarthquakes = features
			.Where(f => f.Properties.Magnitude.HasValue)
			.Select(UsgsMapper.ToEarthquake)
			.ToList();

		if (incomingEarthquakes.Count == 0)
		{
			return 0;
		}

		// 3. Gelenlerden hangileri zaten DB'de var?
		var incomingIds = incomingEarthquakes
			.Select(e => e.ExternalId)
			.ToList();

		var existingIds = (await _db.Earthquakes
			.Where(e => incomingIds.Contains(e.ExternalId))
			.Select(e => e.ExternalId)
			.ToListAsync(cancellationToken))
			.ToHashSet();

		// 4. Sadece yeni olanları ayır
		var newEarthquakes = incomingEarthquakes
			.Where(e => !existingIds.Contains(e.ExternalId))
			.ToList();

		// 5. Kaydet
		_db.Earthquakes.AddRange(newEarthquakes);
		await _db.SaveChangesAsync(cancellationToken);

		_logger.LogInformation(
			"USGS'den {IncomingCount} deprem geldi, {NewCount} tanesi yeni kaydedildi.",
			incomingEarthquakes.Count, newEarthquakes.Count);

		return newEarthquakes.Count;
	}

	//Sayfalandırmalı olarak depremleri getirir.
	public async Task<EarthquakeListViewModel> GetEarthquakesAsync(double? minMagnitude, string? place, int page, CancellationToken cancellationToken = default)
	{
		var query = _db.Earthquakes.AsNoTracking();

		if (minMagnitude.HasValue)
		{
			query = query.Where(e => e.Magnitude >= minMagnitude.Value);
		}

		if (!string.IsNullOrWhiteSpace(place))
		{
			query = query.Where(e => e.Place.Contains(place));
		}

		var result = await query
			.OrderByDescending(e => e.OccurredAtUtc)
			.ToPagedResultAsync(page, cancellationToken: cancellationToken);

		return new EarthquakeListViewModel
		{
			Result = result,
			MinMagnitude = minMagnitude,
			Place = place
		};
	}

	public async Task<Earthquake?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		return await _db.Earthquakes
			.AsNoTracking()
			.Include(e => e.Alert)
			.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
	}
}