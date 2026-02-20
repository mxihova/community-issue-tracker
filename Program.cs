using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Community_Issue_Tracker.Data;

// Create the application builder
var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// SERVICES
// ------------------------------------------------------------

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// SQLite in writable Docker location
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
// BUILD
// ------------------------------------------------------------

var app = builder.Build();

// ------------------------------------------------------------
// MIDDLEWARE
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ------------------------------------------------------------
// 🔥 APPLY MIGRATIONS (PRODUCTION SAFE)
// ------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ------------------------------------------------------------
// START APP
// ------------------------------------------------------------

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");