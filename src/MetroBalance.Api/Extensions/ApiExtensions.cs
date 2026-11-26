using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace MetroBalance.Api.Extensions;

/// <summary>
/// Shared API wiring used by both the standalone Web API host and the MVC host.
/// </summary>
public static class ApiExtensions
{
    /// <summary>Registers API level services (Swagger, JSON options).</summary>
    public static IServiceCollection AddMetroBalanceApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MetroBalance API",
                Version = "v1",
                Description = "Delhi Metro Smart Card balance and transaction history API. " +
                              "All responses are SIMULATED DEMO DATA - no DMRC endpoint is called."
            });
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        // Controllers (not only minimal APIs) must use camelCase so the browser
        // client and the documented contract share one JSON shape.
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });

        return services;
    }

    /// <summary>Adds Swagger UI and the API controllers to the request pipeline.</summary>
    public static WebApplication UseMetroBalanceApi(this WebApplication app)
    {
        app.UseMiddleware<CardExceptionMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MetroBalance API v1");
            options.DocumentTitle = "MetroBalance API";
            options.RoutePrefix = "swagger";
        });

        app.MapControllers();

        return app;
    }
}
