using Vyshkat.Domain;

namespace Vyshkat.Tests;

public class DomainTests
{
    [Fact]
    public void Scooter_IsInventoriableAndPowerConsumer()
    {
        IInventoriable scooter = new KickScooter(" KS-1 ", "Ninebot", 7, 1.25);

        Assert.Equal("KS-1", scooter.InventoryNumber);
        Assert.Equal("Электросамокат «Ninebot»", scooter.Title);
        Assert.True(scooter is IPowerConsumer);
    }

    [Fact]
    public void BicycleHelmetAndDock_AreNotPowerConsumers_ISP()
    {
        IInventoriable[] passive = { new Bicycle("BK-1", "City", 8), new Helmet("HL-1", "m"), new DockingStation("DK-1", 4) };

        Assert.All(passive, x => Assert.False(x is IPowerConsumer));
        Assert.Equal("Шлем защитный (размер M)", passive[1].Title);
        Assert.Equal("Док-станция пассивная, мест: 4", passive[2].Title);
    }

    [Fact]
    public void ChargingStation_IsItemAndPowerConsumer()
    {
        var station = new ChargingStation("CS-1", "Хаб", 14);

        Assert.Equal("Зарядный шкаф «Хаб»", station.Title);
        Assert.Equal(14, station.DailyEnergyKwh);
    }

    [Fact]
    public void InvalidData_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new Bicycle("", "City", 5));
        Assert.Throws<ArgumentException>(() => new Bicycle("BK-1", " ", 5));
        Assert.Throws<ArgumentException>(() => new Bicycle("BK-1", "City", 0));
        Assert.Throws<ArgumentException>(() => new Bicycle("BK-1", "City", 11));
        Assert.Throws<ArgumentException>(() => new EBike("EB-1", "X", 5, -1));
        Assert.Throws<ArgumentException>(() => new KickScooter("KS-1", "X", 5, -1));
        Assert.Throws<ArgumentException>(() => new Helmet(" ", "M"));
        Assert.Throws<ArgumentException>(() => new Helmet("HL-1", ""));
        Assert.Throws<ArgumentException>(() => new DockingStation("DK-1", 0));
        Assert.Throws<ArgumentException>(() => new ChargingStation("CS-1", "", 1));
        Assert.Throws<ArgumentException>(() => new ChargingStation("CS-1", "Хаб", -1));
    }
}
