using EarthquakeTracker.Data;
using EarthquakeTracker.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient<UsgsClient>(client =>
{
	client.BaseAddress = new Uri("https://earthquake.usgs.gov/");
	client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// ---- Middleware (Build'den SONRA) ----
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.Run();
