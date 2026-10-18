namespace Vyshkat.Domain;

// Объект на балансе (Транспорт и Вещи)
public interface IInventoriable
{
    string InventoryNumber { get; }
    string Title { get; }
}

// Потребитель энергии (ISP: выделен отдельно)
public interface IPowerConsumer
{
    double DailyEnergyKwh { get; }
}
