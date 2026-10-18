namespace Vyshkat.Domain;

public abstract class Item : IInventoriable
{
    protected Item(string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber))
            throw new ArgumentException("Инвентарный номер не может быть пустым.");
        InventoryNumber = inventoryNumber.Trim();
    }

    public string InventoryNumber { get; }
    public abstract string Title { get; }
}

public class Helmet : Item
{
    public string Size { get; }

    public Helmet(string inventoryNumber, string size) : base(inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(size))
            throw new ArgumentException("Размер шлема не может быть пустым.");
        Size = size.Trim().ToUpperInvariant();
    }

    public override string Title => $"Шлем защитный (размер {Size})";
}

public class DockingStation : Item
{
    public int Capacity { get; }

    public DockingStation(string inventoryNumber, int capacity) : base(inventoryNumber)
    {
        if (capacity <= 0)
            throw new ArgumentException("Вместимость должна быть > 0.");
        Capacity = capacity;
    }

    public override string Title => $"Док-станция пассивная, мест: {Capacity}";
}

// Зарядный шкаф: вещь И потребитель энергии
public class ChargingStation : Item, IPowerConsumer
{
    public string Name { get; }
    public double DailyEnergyKwh { get; }

    public ChargingStation(string inventoryNumber, string name, double dailyEnergyKwh) : base(inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название зарядного шкафа не может быть пустым.");
        if (dailyEnergyKwh < 0)
            throw new ArgumentException("Энергия не может быть отрицательной.");
        Name = name.Trim();
        DailyEnergyKwh = dailyEnergyKwh;
    }

    public override string Title => $"Зарядный шкаф «{Name}»";
}
