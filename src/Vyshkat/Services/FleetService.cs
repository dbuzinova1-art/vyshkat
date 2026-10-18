using Vyshkat.Domain;

namespace Vyshkat.Services;

public class FleetService
{
    private readonly IFleetRepository _repository;
    private readonly IServiceCenter _serviceCenter;

    // Внедрение зависимостей через конструктор (DI / DIP)
    public FleetService(IFleetRepository repository, IServiceCenter serviceCenter)
    {
        _repository = repository;
        _serviceCenter = serviceCenter;
    }

    // Транспорт попадает в парк только если техосмотр его одобрил
    public InspectionResult RegisterTransport(Transport transport)
    {
        var result = _serviceCenter.Inspect(transport);
        if (result.IsApproved)
            _repository.AddTransport(transport);
        return result;
    }

    public void RegisterItem(Item item) => _repository.AddItem(item);

    public double GetTotalDailyEnergy()
    {
        return Math.Round(
            _repository.GetAllInventory()
                .OfType<IPowerConsumer>()
                .Sum(x => x.DailyEnergyKwh),
            2
        );
    }

    public IReadOnlyList<Transport> GetBeginnerTransports() =>
        _repository.GetAllTransports().Where(t => t.IsBeginnerFriendly()).ToList().AsReadOnly();

    public IReadOnlyList<IInventoriable> GetAllInventory() => _repository.GetAllInventory();
    public (int Transports, int Items) GetCounts() =>
        (_repository.GetAllTransports().Count, _repository.GetAllItems().Count);
}
