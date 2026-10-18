using Microsoft.Extensions.DependencyInjection;
using Vyshkat.Domain;
using Vyshkat.Services;

namespace Vyshkat.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void Container_BuildsApplicationWithStrictServiceCenter()
    {
        using var provider = new ServiceCollection()
            .AddVyshkat()
            .AddConsoleApp(new StringReader(""), new StringWriter(), echoInput: false)
            .BuildServiceProvider();

        Assert.IsType<StrictServiceCenter>(provider.GetRequiredService<IServiceCenter>());
        Assert.NotNull(provider.GetRequiredService<ConsoleApp>());
    }

    [Fact]
    public void ServiceCenter_IsReplacedInContainer()
    {
        // Подмена техосмотра через DI-контейнер: последняя регистрация IServiceCenter
        // побеждает, и FleetService из контейнера получает подставную службу.
        using var provider = new ServiceCollection()
            .AddVyshkat()
            .AddSingleton<IServiceCenter, AlwaysRejectServiceCenter>()
            .BuildServiceProvider();
        var service = provider.GetRequiredService<FleetService>();

        var result = service.RegisterTransport(new KickScooter("KS-1", "Ninebot", 8, 1.2));

        Assert.False(result.IsApproved);
        Assert.Empty(service.GetAllInventory());
    }
}
