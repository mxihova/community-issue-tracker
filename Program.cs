using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Community_Issue_Tracker.Data;

var builder = WebApplication.CreateBuilder(args);

// SERVICES
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

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

// BUILD
var app = builder.Build();

// APPLY MIGRATIONS + SEED ROLES
using (var scope = app.Services.CreateScope())
{
    var scopedServices = scope.ServiceProvider;

    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scopedServices.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "your@email.com";
    var existingUser = await userManager.FindByEmailAsync(adminEmail);

    if (existingUser != null && !await userManager.IsInRoleAsync(existingUser, "Admin"))
    {
        await userManager.AddToRoleAsync(existingUser, "Admin");
    }
}

// MIDDLEWARE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

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

// START APP
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");