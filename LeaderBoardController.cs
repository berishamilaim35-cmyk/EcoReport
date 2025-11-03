using Microsoft.AspNetCore.Mvc;
using EcoReport.Data;
using EcoReport.Models;
using Microsoft.EntityFrameworkCore;

namespace EcoReport.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaderboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Leaderboard
        public async Task<IActionResult> Index()
        {
            // Compute leaderboard dynamically
            var leaderboard = await _context.Users
           .Select(u => new LeaderBoardEntry
            {
             UserName = u.UserName,
             Points = 0, // ose lë 0
             TotalReports = _context.Reports.Count(r => r.UserId == u.Id)
             })
             .OrderByDescending(u => u.Points)
              .ToListAsync();

            return View(leaderboard);
        }
    }
}