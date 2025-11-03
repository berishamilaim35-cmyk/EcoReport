using EcoReport.Data;
using EcoReport.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Add Services
builder.Services.AddControllersWithViews();

// ✅ 2. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ✅ 3. Identity Configuration - KJO ËSHTË SHUMË E RËNDËSISHME
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;

    // SignIn settings
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ✅ 4. Cookie Configuration - KJO ËSHTË GJITHASHTU SHUMË E RËNDËSISHME
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
    options.SlidingExpiration = true;

    // Për development - mos përdor Secure cookies
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ✅ 5. Session Configuration (nëse nevojitet)
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ✅ 6. Configure Pipeline - RENDI ËSHTË SHUMË I RËNDËSISHËM
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ 7. KËTO DUHEN TË VIENË SI KËTU - RENDI I SAKTË
app.UseAuthentication();   // ✅ KJO PARA AUTHORIZATION
app.UseAuthorization();    // ✅ KJO PAS AUTHENTICATION

// ✅ 8. Session (nëse e ke shtuar)
app.UseSession();

// ✅ 9. Debug Middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"=== REQUEST: {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"=== AUTHENTICATED: {context.User.Identity.IsAuthenticated}");
    Console.WriteLine($"=== USER: {context.User.Identity.Name}");
    await next();
});

// ✅ 10. Map Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ 11. KJO ËSHTË SHUMË E RËNDËSISHME PËR IDENTITY
app.MapRazorPages();

app.Run();