using EarthquakeTracker.Data;
using EarthquakeTracker.Jobs;
using EarthquakeTracker.Options;
using EarthquakeTracker.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

// ---------- MVC ----------
builder.Services.AddControllersWithViews();

// ---------- Veritabanı ----------
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(connectionString));

// ---------- Ayarlar ----------appsettings.json(Configure) bölümleri (Usgs, Alert, Job) UsgsOptions, AlertOptions, JobOptions sınıflarına bağlanır
builder.Services.Configure<UsgsOptions>(
	builder.Configuration.GetSection(UsgsOptions.SectionName));

builder.Services.Configure<AlertOptions>(
	builder.Configuration.GetSection(AlertOptions.SectionName));

builder.Services.Configure<JobOptions>(
	builder.Configuration.GetSection(JobOptions.SectionName));

// ---------- Dış servis ----------
builder.Services.AddHttpClient<UsgsClient>((serviceProvider, client) =>
{
	var options = serviceProvider.GetRequiredService<IOptions<UsgsOptions>>().Value;

	client.BaseAddress = new Uri(options.BaseUrl);
	client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// ---------- Uygulama servisleri ----------
builder.Services.AddScoped<EarthquakeService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<FetchEarthquakesJob>();
builder.Services.AddScoped<CheckAlertsJob>();
builder.Services.AddScoped<CleanupOldEarthquakesJob>();

// ---------- Hangfire ----------
builder.Services.AddHangfire(config => config
	.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();

var app = builder.Build();

/*
NOT: Tek seferlik butonla tetiklenen joblar controllerda duruyor çünkü kullanıcıların işlemi ile çalışıyor.
Ama recurring job zamana göre çalışır kullanıcı etkisi gerekmez program.cs uygulama açılırken bir kez çalışır ve zaman kuralları kaydolmuş olur 
 ---------- Recurring job'lar ----------
*/
var jobOptions = app.Services.GetRequiredService<IOptions<JobOptions>>().Value;
var recurringJobs = app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobs.AddOrUpdate<FetchEarthquakesJob>(
	"fetch-earthquakes",//job un adı
	job => job.RunAsync(null, CancellationToken.None), //RunAsync jobu çalışacak
	jobOptions.FetchEarthquakesCron);

recurringJobs.AddOrUpdate<CleanupOldEarthquakesJob>(
	"cleanup-old-earthquakes",
	job => job.RunAsync(CancellationToken.None),
	jobOptions.CleanupCron,
	new RecurringJobOptions
	{
		TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul")
	});
// ---------- Middleware ----------
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");  //Jobları izlemek için /hangfire ekranına gidebiliriz

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Earthquakes}/{action=Index}/{id?}")
	.WithStaticAssets();
app.Run();