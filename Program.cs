using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Community_Issue_Tracker.Data;

// Create the application builder
var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// SERVICE REGISTRATION
// ------------------------------------------------------------

// MVC
builder.Services.AddControllersWithViews();

// Razor Pages (Identity)
builder.Services.AddRazorPages();

// SQLite database in writable Docker location
var dbPath = Path.Combine("/tmp", "community_issues.db");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ------------------------------------------------------------
// BUILD APPLICATION
// ------------------------------------------------------------

var app = builder.Build();

// ------------------------------------------------------------
// MIDDLEWARE PIPELINE
// ------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ------------------------------------------------------------
// 🔥 ENSURE DATABASE IS CREATED (IMPORTANT FOR RENDER)
// ------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// ------------------------------------------------------------
// START APPLICATION (Render compatible)
// ------------------------------------------------------------

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");