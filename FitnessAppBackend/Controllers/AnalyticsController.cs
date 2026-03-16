using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    public class AnalyticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
