using Microsoft.Extensions.DependencyInjection;
using Vyshkat.Services;

namespace Vyshkat.Tests;

public class ConsoleAppTests
{
    // Запускает меню на заданном вводе и возвращает всё, что оно напечатало.
    private static string RunApp(string input, params string[] files)
    {
        var output = new StringWriter();
        using var provider = new ServiceCollection()
            .AddVyshkat()
            .AddConsoleApp(new StringReader(input), output, echoInput: true)
            .BuildServiceProvider();
        var app = provider.GetRequiredService<ConsoleApp>();
        foreach (var file in files)
            app.LoadFile(file);
        app.Run();
        return output.ToString();
    }

    private static string DataFile(string name)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(dir!.FullName, "Vyshkat.sln")))
            dir = dir.Parent;
        return Path.Combine(dir.FullName, "data", name);
    }

    [Fact]
    public void Menu_AddsAllKindsAndShowsReports()
    {
        var output = RunApp(string.Join('\n',
            "1", "1", "KS-1", "Ninebot", "8", "1,5",
            "1", "2", "EB-1", "Eltreco", "4", "2.5",
            "1", "3", "BK-1", "Forward", "9",
            "2", "1", "HL-1", "M",
            "2", "2", "DK-1", "6",
            "2", "3", "CS-1", "Хаб", "10",
            "4", "5", "6", "0"));

        Assert.Contains("Результат: Техосмотр пройден успешно. (Принят: да)", output);
        Assert.Contains("Поставлено на баланс: Док-станция пассивная, мест: 6 [DK-1]", output);
        Assert.Contains("Транспорта: 3, Вещей: 3, Всего: 6", output);
        Assert.Contains($"Суммарный суточный расход энергии: {14.0:0.00} кВт·ч", output);
        Assert.Contains(" - [BK-1] Механический велосипед «Forward» (простота: 9)", output);
        Assert.DoesNotContain(" - [EB-1]", output);
        Assert.Contains(" [CS-1] Зарядный шкаф «Хаб»", output);
        Assert.Contains("Выход.", output);
    }

    [Fact]
    public void Menu_HandlesBadInputAndEndOfInput()
    {
        var output = RunApp(string.Join('\n',
            "9", "5", "6",
            "1", "7",
            "2", "7",
            "1", "1", "KS-0", "Dead", "7", "0",
            "1", "3", "BK-1", "City", "abc",
            "2", "1", "HL-1", " ",
            "3", "нет-такого-файла.txt",
            "1", "3", "BK-2"));

        Assert.Contains("Неизвестный пункт.", output);
        Assert.Contains("Нет техники для новичков.", output);
        Assert.Contains("Баланс пуст.", output);
        Assert.Contains("Неизвестный тип транспорта.", output);
        Assert.Contains("Неизвестный тип вещи.", output);
        Assert.Contains("аккумулятор разряжен (0 кВт·ч). (Принят: нет)", output);
        Assert.Contains("Ошибка: Простота: «abc» — не целое число.", output);
        Assert.Contains("Ошибка: Размер шлема не может быть пустым.", output);
        Assert.Contains("Файл не найден: нет-такого-файла.txt", output);
        // Ввод закончился посреди вопроса — программа завершилась, а не зациклилась
        Assert.EndsWith("Выход." + Environment.NewLine, output);
    }

    [Fact]
    public void FleetFile_LoadsValidLinesAndReportsErrors()
    {
        var output = RunApp("4\n0\n", DataFile("fleet.txt"));

        Assert.Contains("Строка 2: Электросамокат «Ninebot G30» [KS-101] — принят", output);
        Assert.Contains("Строка 3: Электросамокат «Broken M365» [KS-BAD] — отклонён", output);
        Assert.Contains("Строка 8: Зарядный шкаф «Покровка-Хаб» [CS-03] — на балансе", output);
        Assert.Contains("Строка 11: ошибка — Инвентарный номер 'KS-101' уже занят.", output);
        Assert.Contains("Строка 12: ошибка — Простота должна быть от 1 до 10.", output);
        Assert.Contains("Строка 13: ошибка — не удалось разобрать", output);
        Assert.Contains("Транспорта: 3, Вещей: 3, Всего: 6", output);
    }

    [Fact]
    public void Loader_ParsesEveryKindAndRejectsWrongFieldCount()
    {
        var service = new FleetService(new InMemoryFleetRepository(), new AlwaysAcceptServiceCenter());
        var loader = new FleetFileLoader(service);

        var messages = loader.Load(new StringReader(string.Join('\n',
            "scooter;KS-1;A;8;1.5", "EBIKE;EB-1;B;4;2,5", "bicycle;BK-1;C;9",
            "helmet;HL-1;L", "dock;DK-1;3", "charger;CS-1;Хаб;12", "dock;DK-2", "charger;CS-2;Хаб;много")));

        Assert.Equal(6, service.GetAllInventory().Count);
        Assert.Equal(16, service.GetTotalDailyEnergy());
        Assert.Contains("ошибка — не удалось разобрать", messages[6]);
        Assert.Contains("Расход: «много» — не число.", messages[7]);
    }
}
