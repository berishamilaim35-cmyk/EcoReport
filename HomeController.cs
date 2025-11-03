using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EcoReport.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // ✅ HOME PAGE - PUBLIKE për të gjithë
        [AllowAnonymous] // Kjo garanton që është publike
        public IActionResult Index()
        {
            _logger.LogInformation($"🏠 Home page accessed - User: {User.Identity.Name}, Authenticated: {User.Identity.IsAuthenticated}");

            // Debug info në console
            Console.WriteLine("=== HOME PAGE ACCESSED ===");
            Console.WriteLine($"User: {User.Identity.Name}");
            Console.WriteLine($"Authenticated: {User.Identity.IsAuthenticated}");
            Console.WriteLine("==========================");

            return View();
        }

        // ✅ PRIVACY PAGE - PUBLIKE për të gjithë
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        // ✅ ABOUT PAGE - PUBLIKE për të gjithë
        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        // ✅ ERROR PAGE - PUBLIKE për të gjithë
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        // ✅ TEST PAGE - për debug
        [AllowAnonymous]
        public IActionResult Test()
        {
            var model = new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
                CurrentTime = DateTime.Now
            };

            return Json(model);
        }
    }
}