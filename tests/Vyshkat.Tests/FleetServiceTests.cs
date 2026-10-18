using Vyshkat.Domain;
using Vyshkat.Services;

namespace Vyshkat.Tests;

public class FleetServiceTests
{
    private static FleetService CreateService(IServiceCenter serviceCenter) =>
        new(new InMemoryFleetRepository(), serviceCenter);

    [Fact]
    public void RegisterTransport_WhenServiceCenterRejects_DoesNotAddToFleet()
    {
        // КЛЮЧЕВОЙ ТЕСТ: подмена техосмотра через конструктор на отказ
        var service = CreateService(new AlwaysRejectServiceCenter());
        var scooter = new KickScooter("KS-1", "Test", 8, 1.5);

        var result = service.RegisterTransport(scooter);

        Assert.False(result.IsApproved);
        Assert.Contains("Отклонено", result.Reason);
        Assert.Empty(service.GetAllInventory());
    }

    [Fact]
    public void RegisterTransport_WhenServiceCenterAccepts_AddsToFleet()
    {
        // Даже «разряженный» самокат принят: решение принимает подставленная служба
        var service = CreateService(new AlwaysAcceptServiceCenter());

        var result = service.RegisterTransport(new KickScooter("KS-1", "Test", 8, 0));

        Assert.True(result.IsApproved);
        Assert.Single(service.GetAllInventory());
    }

    [Fact]
    public void StrictServiceCenter_RejectsDischargedAndApprovesWorking()
    {
        var center = new StrictServiceCenter();

        var discharged = center.Inspect(new KickScooter("KS-0", "Zero", 7, 0.0));

        Assert.False(discharged.IsApproved);
        Assert.Contains("аккумулятор разряжен", discharged.Reason);
        Assert.True(center.Inspect(new EBike("EB-1", "Ok", 7, 2)).IsApproved);
        Assert.True(center.Inspect(new Bicycle("BK-1", "City", 7)).IsApproved);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(5, false)]
    [InlineData(6, true)]
    [InlineData(10, true)]
    public void BeginnerRule_ChecksBoundaryConditions(int score, bool expected)
    {
        var service = CreateService(new AlwaysAcceptServiceCenter());
        var scooter = new KickScooter("KS-" + score, "Model", score, 1.0);

        service.RegisterTransport(scooter);

        Assert.Equal(expected, service.GetBeginnerTransports().Contains(scooter));
    }

    [Fact]
    public void TotalDailyEnergy_OnlySumsPowerConsumers_ISP()
    {
        var service = CreateService(new AlwaysAcceptServiceCenter());
        service.RegisterTransport(new KickScooter("KS-1", "Ninebot", 7, 2.0));
        service.RegisterTransport(new Bicycle("BK-1", "Forward", 8));   // 0 кВт·ч
        service.RegisterItem(new Helmet("HL-1", "M"));                   // 0 кВт·ч
        service.RegisterItem(new DockingStation("DK-1", 5));             // 0 кВт·ч
        service.RegisterItem(new ChargingStation("CS-1", "Hub", 10.5));  // 10.5 кВт·ч

        Assert.Equal(12.5, service.GetTotalDailyEnergy());
        Assert.Equal((2, 3), service.GetCounts());
    }

    [Fact]
    public void DuplicateInventoryNumber_IsRejected()
    {
        var service = CreateService(new AlwaysAcceptServiceCenter());
        service.RegisterItem(new Helmet("INV-1", "M"));

        Assert.Throws<InvalidOperationException>(() => service.RegisterTransport(new Bicycle("INV-1", "City", 7)));
        Assert.Throws<InvalidOperationException>(() => service.RegisterItem(new DockingStation("INV-1", 5)));
        Assert.Single(service.GetAllInventory());
    }

    [Fact]
    public void Extensibility_OCP_CanAddNewTransportTypeWithoutModifyingService()
    {
        // Новый вид транспорта — один класс, FleetService не меняется
        var service = CreateService(new StrictServiceCenter());

        service.RegisterTransport(new FakeMonowheel("MW-1", "InMotion", 5));

        Assert.Single(service.GetAllInventory());
        Assert.Equal("Моноколесо «InMotion»", service.GetAllInventory()[0].Title);
    }

    private class FakeMonowheel : Transport
    {
        public FakeMonowheel(string inv, string mod, int sc) : base(inv, mod, sc) { }
        public override string TypeName => "Моноколесо";
    }
}
