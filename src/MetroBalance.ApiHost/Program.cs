using MetroBalance.Api.Extensions;
using MetroBalance.Api.Controllers;
using MetroBalance.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMetroBalanceInfrastructure();
builder.Services.AddMetroBalanceApi();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(CardsController).Assembly);

var app = builder.Build();

app.UseMetroBalanceApi();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
