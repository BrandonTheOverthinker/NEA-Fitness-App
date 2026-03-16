using Microsoft.AspNetCore.Mvc;

namespace FitnessAppBackend.Controllers
{
    public class GoalController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
