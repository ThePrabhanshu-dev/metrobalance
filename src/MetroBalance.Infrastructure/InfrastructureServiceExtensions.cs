using MetroBalance.Core.Interfaces;
using MetroBalance.Infrastructure.Data;
using MetroBalance.Infrastructure.Providers;
using MetroBalance.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MetroBalance.Infrastructure;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers the in-memory data store, the simulated card provider and the card services.
    /// </summary>
    public static IServiceCollection AddMetroBalanceInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryCardStore>();
        services.AddSingleton<IMetroCardProvider, MockMetroCardProvider>();
        services.AddSingleton<ICardValidator, CardValidator>();
        services.AddScoped<ICardService, CardService>();

        return services;
    }
}
