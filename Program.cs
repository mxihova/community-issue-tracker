using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Community_Issue_Tracker.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================
// SERVICES
// ============================

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Get connection string (Render uses DATABASE_URL)
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("DATABASE_URL is not configured.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

// ============================
// BUILD
// ============================

var app = builder.Build();

// ============================
// APPLY MIGRATIONS + SEED ROLES
// ============================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Optional: auto-assign Admin role to a specific email
    var adminEmail = "your@email.com";
    var existingUser = await userManager.FindByEmailAsync(adminEmail);

    if (existingUser != null && !await userManager.IsInRoleAsync(existingUser, "Admin"))
    {
        await userManager.AddToRoleAsync(existingUser, "Admin");
    }
}

// ============================
// MIDDLEWARE
// ============================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Avoid HTTPS redirect issues on Render
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ============================
// START APP (Render Compatible)
// ============================

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");