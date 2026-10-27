using Microsoft.AspNetCore.Mvc;

namespace EarthquakeTracker.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
