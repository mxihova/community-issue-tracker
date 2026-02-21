using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Community_Issue_Tracker.Data;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// SERVICES
// ------------------------------------------------------------

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 🔥 Use persistent container path
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "community_issues.db");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

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
// APPLY MIGRATIONS BEFORE ANY REQUESTS
// ------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

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
// START APP
// ------------------------------------------------------------

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");