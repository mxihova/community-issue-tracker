using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Community_Issue_Tracker.Data;

// Create the application builder (bootstraps config + services)
var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// SERVICE REGISTRATION SECTION
// Everything here registers dependencies into the DI container
// ------------------------------------------------------------

// Add MVC controllers + views support
// This enables the MVC pattern (Controllers + Razor Views)
builder.Services.AddControllersWithViews();

// Add Razor Pages support (required for Identity UI pages)
builder.Services.AddRazorPages();

// Build an absolute file path for the SQLite database file
// This ensures EF migrations and runtime both use the same physical file location
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "community_issues.db");

// Print actual resolved database file path

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));


// Register ASP.NET Core Identity system
// Provides user accounts, login, password hashing, cookies
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Configure basic password rules (good for demo/learning apps)
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ------------------------------------------------------------
// BUILD APPLICATION
// ------------------------------------------------------------
var app = builder.Build();

// ------------------------------------------------------------
// MIDDLEWARE PIPELINE SECTION
// Order matters here — request flows through this pipeline
// ------------------------------------------------------------

// Production error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Force HTTPS
app.UseHttpsRedirection();

// Serve static files (css/js/images)
app.UseStaticFiles();

// Enable routing system
app.UseRouting();

// Enable authentication (reads login cookie → sets User)
app.UseAuthentication();

// Enable authorization (checks access rules)
app.UseAuthorization();

// Map MVC controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Identity Razor Pages endpoints
app.MapRazorPages();

// ------------------------------------------------------------
// START APPLICATION
// ------------------------------------------------------------
app.Run();
