using MetroBalance.Api.Extensions;
using MetroBalance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMetroBalanceInfrastructure();
builder.Services.AddMetroBalanceApi();
builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(MetroBalance.Api.Controllers.CardsController).Assembly);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// UseDefaultFiles lets folder URLs (for example /demo/) resolve to their
// index.html, exactly the way the GitHub Pages copy of the demo behaves.
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

// Swagger UI is available on the running site at /swagger.
app.UseMetroBalanceApi();

app.MapControllerRoute(
    name: "showcase",
    pattern: "showcase",
    defaults: new { controller = "Home", action = "Showcase" });

app.MapControllerRoute(
    name: "guide",
    pattern: "guide",
    defaults: new { controller = "Home", action = "Guide" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
