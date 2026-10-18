using Vyshkat.Domain;

namespace Vyshkat.Services;

public interface IFleetRepository
{
    void AddTransport(Transport transport);
    void AddItem(Item item);
    IReadOnlyList<Transport> GetAllTransports();
    IReadOnlyList<Item> GetAllItems();
    IReadOnlyList<IInventoriable> GetAllInventory();
}

public class InMemoryFleetRepository : IFleetRepository
{
    private readonly List<Transport> _transports = new();
    private readonly List<Item> _items = new();

    private void EnsureUnique(string inventoryNumber)
    {
        if (GetAllInventory().Any(x => x.InventoryNumber == inventoryNumber))
            throw new InvalidOperationException($"Инвентарный номер '{inventoryNumber}' уже занят.");
    }

    public void AddTransport(Transport transport)
    {
        EnsureUnique(transport.InventoryNumber);
        _transports.Add(transport);
    }

    public void AddItem(Item item)
    {
        EnsureUnique(item.InventoryNumber);
        _items.Add(item);
    }

    public IReadOnlyList<Transport> GetAllTransports() => _transports.AsReadOnly();
    public IReadOnlyList<Item> GetAllItems() => _items.AsReadOnly();
    public IReadOnlyList<IInventoriable> GetAllInventory() => _transports.Cast<IInventoriable>().Concat(_items).ToList().AsReadOnly();
}
