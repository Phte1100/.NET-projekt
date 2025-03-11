using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using projekt.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";  // Standard inloggningsväg
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Standard vid nekad åtkomst
    options.LogoutPath = "/Identity/Account/Logout"; // Standard Logout URL
    options.Events.OnRedirectToLogout = context =>
    {
        context.Response.Redirect("/Ad/Index"); // Omdirigera till annonserna efter utloggning
        return Task.CompletedTask;
    };
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Ad}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
