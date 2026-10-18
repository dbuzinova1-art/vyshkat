namespace Vyshkat.Domain;

public abstract class Transport : IInventoriable
{
    protected Transport(string inventoryNumber, string model, int simplicityScore)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber))
            throw new ArgumentException("Инвентарный номер не может быть пустым.");
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Модель не может быть пустой.");
        if (simplicityScore is < 1 or > 10)
            throw new ArgumentException("Простота должна быть от 1 до 10.");

        InventoryNumber = inventoryNumber.Trim();
        Model = model.Trim();
        SimplicityScore = simplicityScore;
    }

    public string InventoryNumber { get; }
    public string Model { get; }
    public int SimplicityScore { get; }
    public abstract string TypeName { get; }
    public string Title => $"{TypeName} «{Model}»";

    // Требование ТЗ: простота >= 6
    public bool IsBeginnerFriendly() => SimplicityScore >= 6;
}

// Электросамокат: транспорт + потребитель энергии
public class KickScooter : Transport, IPowerConsumer
{
    public KickScooter(string inventoryNumber, string model, int simplicityScore, double dailyEnergyKwh)
        : base(inventoryNumber, model, simplicityScore)
    {
        if (dailyEnergyKwh < 0)
            throw new ArgumentException("Энергия не может быть отрицательной.");
        DailyEnergyKwh = dailyEnergyKwh;
    }

    public double DailyEnergyKwh { get; }
    public override string TypeName => "Электросамокат";
}

// Электровелосипед: транспорт + потребитель энергии
public class EBike : Transport, IPowerConsumer
{
    public EBike(string inventoryNumber, string model, int simplicityScore, double dailyEnergyKwh)
        : base(inventoryNumber, model, simplicityScore)
    {
        if (dailyEnergyKwh < 0)
            throw new ArgumentException("Энергия не может быть отрицательной.");
        DailyEnergyKwh = dailyEnergyKwh;
    }

    public double DailyEnergyKwh { get; }
    public override string TypeName => "Электровелосипед";
}

// Механический велосипед: НЕ реализует IPowerConsumer 
public class Bicycle : Transport
{
    public Bicycle(string inventoryNumber, string model, int simplicityScore)
        : base(inventoryNumber, model, simplicityScore) { }

    public override string TypeName => "Механический велосипед";
}
