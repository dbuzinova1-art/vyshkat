using Microsoft.Extensions.DependencyInjection;

namespace Vyshkat;

public static class Program
{
    // Точка входа: собирает приложение через DI-контейнер и запускает меню.
    // Файлы, переданные аргументами, загружаются до открытия меню:
    //   dotnet run --project src/Vyshkat -- data/fleet.txt
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        if (Console.IsInputRedirected)
            Console.InputEncoding = System.Text.Encoding.UTF8;

        using var provider = new ServiceCollection()
            .AddVyshkat()
            .AddConsoleApp(Console.In, Console.Out, echoInput: Console.IsInputRedirected)
            .BuildServiceProvider();

        var app = provider.GetRequiredService<ConsoleApp>();
        foreach (var path in args)
            app.LoadFile(path);

        app.Run();
    }
}
