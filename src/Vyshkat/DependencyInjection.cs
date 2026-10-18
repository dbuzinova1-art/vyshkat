using Microsoft.Extensions.DependencyInjection;
using Vyshkat.Services;

namespace Vyshkat;

// Регистрация зависимостей в стандартном DI-контейнере .NET.
// Только здесь выбираются конкретные реализации интерфейсов.
public static class DependencyInjection
{
    public static IServiceCollection AddVyshkat(this IServiceCollection services)
    {
        services.AddSingleton<IFleetRepository, InMemoryFleetRepository>();
        services.AddSingleton<IServiceCenter, StrictServiceCenter>();
        services.AddSingleton<FleetService>();
        services.AddSingleton<FleetFileLoader>();
        return services;
    }

    public static IServiceCollection AddConsoleApp(this IServiceCollection services, TextReader input, TextWriter output, bool echoInput)
    {
        services.AddSingleton(sp => new ConsoleApp(
            sp.GetRequiredService<FleetService>(),
            sp.GetRequiredService<FleetFileLoader>(),
            input, output, echoInput));
        return services;
    }
}
