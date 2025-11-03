using EcoReport.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoReport.Controllers
{
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .Where(r => r.Latitude != null && r.Longitude != null)
                .ToListAsync();

            return View(reports); // pass reports with coordinates to the view
        }
    }
}
