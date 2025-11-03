using EcoReport.Data;
using EcoReport.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcoReport.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReportsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 📄 Të gjitha raportet
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("=== DEBUG INDEX START ===");

                var canConnect = await _context.Database.CanConnectAsync();
                Console.WriteLine($"Database can connect: {canConnect}");

                if (!canConnect)
                {
                    ViewBag.Error = "Nuk mund të lidhem me databazën!";
                    return View(new List<Report>());
                }

                var reports = await _context.Reports
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                Console.WriteLine($"Returning {reports.Count} reports to view");
                Console.WriteLine("=== DEBUG INDEX END ===");

                return View(reports);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR IN INDEX: {ex.Message} ===");
                ViewBag.Error = $"Gabim në loadimin e raporteve: {ex.Message}";
                return View(new List<Report>());
            }
        }

        // 🆕 Krijo raport të ri (GET)
        [HttpGet]
        public IActionResult Create()
        {
            Console.WriteLine("=== DEBUG CREATE GET ===");
            Console.WriteLine($"🔐 User: {User.Identity?.Name}, Authenticated: {User.Identity?.IsAuthenticated}");
            return View();
        }

        // 🆕 Krijo raport të ri (POST) - VERSIONI I PËRMIRËSUAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Report report)
        {
            Console.WriteLine("🎯 === CREATE POST STARTED ===");

            // DEBUG: Shfaq të gjitha vlerat e marra
            Console.WriteLine("📄 FORM DATA RECEIVED:");
            Console.WriteLine($"   - Title: '{report.Title}'");
            Console.WriteLine($"   - City: '{report.City}'");
            Console.WriteLine($"   - Location: '{report.Location}'");
            Console.WriteLine($"   - WasteType: '{report.WasteType}'");
            Console.WriteLine($"   - Description: '{report.Description}'");

            // Kontrollo manualisht fushat e detyrueshme
            if (string.IsNullOrWhiteSpace(report.Title))
            {
                ModelState.AddModelError("Title", "Titulli është i detyrueshëm");
            }
            if (string.IsNullOrWhiteSpace(report.City))
            {
                ModelState.AddModelError("City", "Qyteti është i detyrueshëm");
            }
            if (string.IsNullOrWhiteSpace(report.Location))
            {
                ModelState.AddModelError("Location", "Lokacioni është i detyrueshëm");
            }
            if (string.IsNullOrWhiteSpace(report.WasteType))
            {
                ModelState.AddModelError("WasteType", "Lloji i mbeturinave është i detyrueshëm");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState is INVALID - showing errors:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"   - {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View(report);
            }

            try
            {
                Console.WriteLine("✅ ModelState is valid - proceeding with save...");

                var currentUser = await _userManager.GetUserAsync(User);
                Console.WriteLine($"👤 Current User from DB: {currentUser?.UserName}, ID: {currentUser?.Id}");

                if (currentUser == null)
                {
                    Console.WriteLine("❌ User is NULL - cannot create report");
                    TempData["ErrorMessage"] = "Përdoruesi nuk u gjet! Ju lutem identifikohuni përsëri.";
                    return RedirectToAction("Login", "Account");
                }

                // Krijo një INSTANCË TË RE të Report për të shmangur konfliktet
                //AAAA chatiiiii atyyyy
                var newReport = new Report
                {
                    Title = report.Title.Trim(),//qka bon qekjo .Trim() ???????????????????????
                    City = report.City.Trim(),
                    Location = report.Location.Trim(),
                    WasteType = report.WasteType,
                    Description = report.Description?.Trim(),
                    Latitude = report.Latitude,
                    Longitude = report.Longitude,
                    // Të dhënat e sistemit
                    UserId = currentUser.Id,
                    UploadedBy = currentUser.UserName ?? currentUser.Email ?? "Unknown",
                    CreatedAt = DateTime.Now,
                    DateReported = DateTime.Now,
                    Status = "Pending",
                    Created = DateTime.Now
                };

                Console.WriteLine("💾 Report data prepared for saving:");
                Console.WriteLine($"   - Title: {newReport.Title}");
                Console.WriteLine($"   - City: {newReport.City}");
                Console.WriteLine($"   - Location: {newReport.Location}");
                Console.WriteLine($"   - WasteType: {newReport.WasteType}");
                Console.WriteLine($"   - UserId: {newReport.UserId}");
                Console.WriteLine($"   - UploadedBy: {newReport.UploadedBy}");

                // Shto në databazë
                Console.WriteLine("💾 Adding report to database context...");
                _context.Reports.Add(newReport);

                Console.WriteLine("🔄 Calling SaveChangesAsync...");
                int recordsAffected = await _context.SaveChangesAsync();

                Console.WriteLine($"✅ SAVE SUCCESSFUL! Records affected: {recordsAffected}");
                Console.WriteLine($"🆕 New Report ID: {newReport.Id}");

                // Verifikim i menjëhershëm
                var savedReport = await _context.Reports.FindAsync(newReport.Id);
                if (savedReport != null)
                {
                    Console.WriteLine($"✅ VERIFIED: Report found in database with ID: {savedReport.Id}");
                    TempData["SuccessMessage"] = $"Raporti u krijua me sukses! ID: {savedReport.Id}";
                }
                else
                {
                    Console.WriteLine("❌ WARNING: Report not found after save");
                    TempData["ErrorMessage"] = "Raporti u krijua por verifikimi dështoi!";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"💥 DB UPDATE EXCEPTION: {dbEx.Message}");
                Console.WriteLine($"🔍 Inner Exception: {dbEx.InnerException?.Message}");

                // Gabime specifike për MySQL
                if (dbEx.InnerException?.Message.Contains("Duplicate entry") == true)
                {
                    ModelState.AddModelError("", "Ky raport ekziston tashmë!");
                }
                else if (dbEx.InnerException?.Message.Contains("foreign key constraint") == true)
                {
                    ModelState.AddModelError("", "Gabim në lidhjen me përdoruesin!");
                }
                else
                {
                    ModelState.AddModelError("", $"Gabim në databazë: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 GENERAL EXCEPTION: {ex.Message}");
                Console.WriteLine($"📋 Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", $"Gabim i papritur: {ex.Message}");
            }

            Console.WriteLine("🏁 === CREATE POST ENDED WITH ERRORS ===");
            return View(report);
        }

        // 🔍 Detajet e një raporti
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                Console.WriteLine($"=== DEBUG DETAILS START - ID: {id} ===");

                var report = await _context.Reports
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (report == null)
                {
                    Console.WriteLine($"=== REPORT NOT FOUND - ID: {id} ===");
                    TempData["ErrorMessage"] = "Raporti nuk u gjet!";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"=== FOUND REPORT: {report.Title} ===");
                return View(report);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR IN DETAILS: {ex.Message} ===");
                TempData["ErrorMessage"] = "Gabim në loadimin e detajeve!";
                return RedirectToAction(nameof(Index));
            }
        }

        // ❌ Fshi raport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Console.WriteLine($"=== DEBUG DELETE START - ID: {id} ===");

                var report = await _context.Reports.FindAsync(id);
                if (report == null)
                {
                    Console.WriteLine($"=== REPORT NOT FOUND FOR DELETE - ID: {id} ===");
                    TempData["ErrorMessage"] = "Raporti nuk u gjet!";
                    return RedirectToAction(nameof(Index));
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (report.UserId != currentUser?.Id && !User.IsInRole("Admin"))
                {
                    Console.WriteLine($"=== UNAUTHORIZED DELETE ATTEMPT ===");
                    TempData["ErrorMessage"] = "Ju mund të fshini vetëm raportet tuaja!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();

                Console.WriteLine($"=== SUCCESS: Report deleted - ID: {id} ===");
                TempData["SuccessMessage"] = "Raporti u fshi me sukses!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR IN DELETE: {ex.Message} ===");
                TempData["ErrorMessage"] = "Gabim në fshirjen e raportit!";
                return RedirectToAction(nameof(Index));
            }
        }

        // 🛠️ Action për debug - vetëm për development
        [HttpGet]
        public async Task<IActionResult> Debug()
        {
            var info = new
            {
                DatabaseConnected = await _context.Database.CanConnectAsync(),
                TotalReports = await _context.Reports.CountAsync(),
                AllReports = await _context.Reports
                    .Select(r => new { r.Id, r.Title, r.City, r.CreatedAt, r.Status })
                    .Take(10)
                    .ToListAsync(),
                CurrentUser = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                UserId = _userManager.GetUserId(User)
            };

            return Json(info);
        }

        // 🧪 Krijo raport test automatikisht - VERSION I RI
        [HttpPost]
        public async Task<IActionResult> CreateTestReport()
        {
            try
            {
                Console.WriteLine("🧪 === CREATING TEST REPORT ===");

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, error = "User not found" });
                }

                var testReport = new Report
                {
                    Title = "Test Raport " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    City = "Prishtinë",
                    Location = "Rruga Test " + Guid.NewGuid().ToString()[..8],
                    WasteType = "Plastikë",
                    Description = "Ky është një raport test i krijuar automatikisht",
                    UserId = currentUser.Id,
                    UploadedBy = currentUser.UserName ?? "System",
                    CreatedAt = DateTime.Now,
                    DateReported = DateTime.Now,
                    Status = "Pending",
                    Created = DateTime.Now
                };

                _context.Reports.Add(testReport);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ TEST REPORT CREATED: ID {testReport.Id}");
                return Json(new { success = true, id = testReport.Id, message = "Raport test u krijua me sukses!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 TEST REPORT ERROR: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // 🔄 Kontrollo statusin e databazës
        [HttpGet]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var reportsCount = await _context.Reports.CountAsync();
                var lastReport = await _context.Reports
                    .OrderByDescending(r => r.Id)
                    .Select(r => new { r.Id, r.Title, r.CreatedAt })
                    .FirstOrDefaultAsync();

                return Json(new
                {
                    databaseConnected = canConnect,
                    totalReports = reportsCount,
                    lastReport = lastReport,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}