using Vyshkat.Domain;
using Vyshkat.Services;

namespace Vyshkat;

// Консольное меню: только ввод и вывод. Решения принимает FleetService.
// Потоки ввода/вывода передаются снаружи, поэтому меню работает и с клавиатурой,
// и с файлом-сценарием (dotnet run < data/demo-session.txt), и в тестах.
public class ConsoleApp
{
    private readonly FleetService _service;
    private readonly FleetFileLoader _loader;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly bool _echoInput;

    public ConsoleApp(FleetService service, FleetFileLoader loader, TextReader input, TextWriter output, bool echoInput)
    {
        _service = service;
        _loader = loader;
        _input = input;
        _output = output;
        _echoInput = echoInput;
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();
            var choice = Ask("Выбор: ");
            if (choice is null || choice == "0")
                break;

            try
            {
                switch (choice)
                {
                    case "1": AddTransport(); break;
                    case "2": AddItem(); break;
                    case "3": LoadFile(Ask("Путь к файлу: ")); break;
                    case "4": PrintSummary(); break;
                    case "5": PrintBeginners(); break;
                    case "6": PrintInventory(); break;
                    default: _output.WriteLine("Неизвестный пункт."); break;
                }
            }
            catch (EndOfStreamException)
            {
                break;
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException or InvalidOperationException)
            {
                _output.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        _output.WriteLine("Выход.");
    }

    public void LoadFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _output.WriteLine($"Файл не найден: {path}");
            return;
        }

        using var reader = new StreamReader(path);
        foreach (var message in _loader.Load(reader))
            _output.WriteLine(message);
    }

    private void PrintMenu()
    {
        _output.WriteLine("\n--- Меню «ВышКат» ---");
        _output.WriteLine("1. Принять транспорт (с техосмотром)");
        _output.WriteLine("2. Поставить вещь на баланс");
        _output.WriteLine("3. Загрузить технику и вещи из файла");
        _output.WriteLine("4. Отчёт: количество и энергия");
        _output.WriteLine("5. Список для новичков (простота >= 6)");
        _output.WriteLine("6. Полный инвентарь");
        _output.WriteLine("0. Выход");
    }

    private void AddTransport()
    {
        var kind = Require("Тип (1 — электросамокат, 2 — электровелосипед, 3 — велосипед): ");
        if (kind is not ("1" or "2" or "3"))
        {
            _output.WriteLine("Неизвестный тип транспорта.");
            return;
        }

        var inv = Require("Инв. №: ");
        var model = Require("Модель: ");
        var score = InputParser.ParseInt(Require("Простота (1-10): "), "Простота");

        Transport transport = kind switch
        {
            "1" => new KickScooter(inv, model, score, AskEnergy()),
            "2" => new EBike(inv, model, score, AskEnergy()),
            _ => new Bicycle(inv, model, score),
        };

        var result = _service.RegisterTransport(transport);
        _output.WriteLine($"Результат: {result.Reason} (Принят: {(result.IsApproved ? "да" : "нет")})");
    }

    private double AskEnergy() => InputParser.ParseDouble(Require("Расход (кВт·ч): "), "Расход");

    private void AddItem()
    {
        var kind = Require("Тип (1 — шлем, 2 — док-станция, 3 — зарядный шкаф): ");
        Item item;
        switch (kind)
        {
            case "1":
                item = new Helmet(Require("Инв. №: "), Require("Размер: "));
                break;
            case "2":
                item = new DockingStation(Require("Инв. №: "), InputParser.ParseInt(Require("Число мест: "), "Число мест"));
                break;
            case "3":
                var inv = Require("Инв. №: ");
                var name = Require("Название: ");
                item = new ChargingStation(inv, name, InputParser.ParseDouble(Require("Расход кВт·ч: "), "Расход"));
                break;
            default:
                _output.WriteLine("Неизвестный тип вещи.");
                return;
        }

        _service.RegisterItem(item);
        _output.WriteLine($"Поставлено на баланс: {item.Title} [{item.InventoryNumber}]");
    }

    private void PrintSummary()
    {
        var (transports, items) = _service.GetCounts();
        _output.WriteLine($"Транспорта: {transports}, Вещей: {items}, Всего: {transports + items}");
        _output.WriteLine($"Суммарный суточный расход энергии: {_service.GetTotalDailyEnergy():0.00} кВт·ч");
    }

    private void PrintBeginners()
    {
        var beginners = _service.GetBeginnerTransports();
        if (beginners.Count == 0)
            _output.WriteLine("Нет техники для новичков.");
        foreach (var t in beginners)
            _output.WriteLine($" - [{t.InventoryNumber}] {t.Title} (простота: {t.SimplicityScore})");
    }

    private void PrintInventory()
    {
        var inventory = _service.GetAllInventory();
        if (inventory.Count == 0)
            _output.WriteLine("Баланс пуст.");
        foreach (var x in inventory)
            _output.WriteLine($" [{x.InventoryNumber}] {x.Title}");
    }

    // null — ввод закончился (Ctrl+D / конец файла-сценария).
    private string? Ask(string prompt)
    {
        _output.Write(prompt);
        var answer = _input.ReadLine();
        if (_echoInput && answer != null)
            _output.WriteLine(answer);
        return answer?.Trim();
    }

    private string Require(string prompt) => Ask(prompt) ?? throw new EndOfStreamException();
}
