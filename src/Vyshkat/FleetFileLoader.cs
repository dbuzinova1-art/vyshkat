using Vyshkat.Domain;
using Vyshkat.Services;

namespace Vyshkat;

// Загрузка техники и вещей из текстового файла. Одна строка — один объект:
//   scooter;KS-101;Ninebot G30;8;1.2      (вид;инв. №;модель;простота;расход кВт·ч)
//   ebike;EB-301;Kupers Pro;4;2.5
//   bicycle;BK-201;Forward City;9
//   helmet;HL-01;L                       (вид;инв. №;размер)
//   dock;DK-02;10                        (вид;инв. №;число мест)
//   charger;CS-03;Покровка-Хаб;15        (вид;инв. №;название;расход кВт·ч)
// Пустые строки и строки с # пропускаются. Ошибка в одной строке не мешает остальным.
public class FleetFileLoader
{
    private readonly FleetService _service;

    public FleetFileLoader(FleetService service)
    {
        _service = service;
    }

    // Возвращает по сообщению на каждую строку с данными.
    public List<string> Load(TextReader reader)
    {
        var messages = new List<string>();
        var lineNumber = 0;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            try
            {
                messages.Add($"Строка {lineNumber}: {Register(ParseLine(line))}");
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException or InvalidOperationException)
            {
                messages.Add($"Строка {lineNumber}: ошибка — {ex.Message}");
            }
        }

        return messages;
    }

    public static IInventoriable ParseLine(string line)
    {
        var p = line.Split(';').Select(x => x.Trim()).ToArray();
        return p[0].ToLowerInvariant() switch
        {
            "scooter" when p.Length == 5 => new KickScooter(p[1], p[2], InputParser.ParseInt(p[3], "Простота"), InputParser.ParseDouble(p[4], "Расход")),
            "ebike" when p.Length == 5 => new EBike(p[1], p[2], InputParser.ParseInt(p[3], "Простота"), InputParser.ParseDouble(p[4], "Расход")),
            "bicycle" when p.Length == 4 => new Bicycle(p[1], p[2], InputParser.ParseInt(p[3], "Простота")),
            "helmet" when p.Length == 3 => new Helmet(p[1], p[2]),
            "dock" when p.Length == 3 => new DockingStation(p[1], InputParser.ParseInt(p[2], "Число мест")),
            "charger" when p.Length == 4 => new ChargingStation(p[1], p[2], InputParser.ParseDouble(p[3], "Расход")),
            _ => throw new FormatException($"не удалось разобрать «{line}»: неизвестный вид или неверное число полей."),
        };
    }

    private string Register(IInventoriable asset)
    {
        if (asset is Transport transport)
        {
            var result = _service.RegisterTransport(transport);
            return $"{transport.Title} [{transport.InventoryNumber}] — {(result.IsApproved ? "принят" : "отклонён")}: {result.Reason}";
        }

        _service.RegisterItem((Item)asset);
        return $"{asset.Title} [{asset.InventoryNumber}] — на балансе";
    }
}
