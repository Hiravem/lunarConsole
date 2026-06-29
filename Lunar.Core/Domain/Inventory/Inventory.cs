namespace Lunar.Core.Domain.Inventory;

public sealed class Inventory
{
    public const int DefaultCapacity = 20;

    private readonly List<ItemStack> _slots = new();
    public int Capacity { get; }

    public Inventory(int capacity = DefaultCapacity) => Capacity = capacity;

    public IReadOnlyList<ItemStack> Items => _slots;

    public bool HasSpace() => TotalSlotCount() < Capacity;

    public int GetQuantity(string itemId) =>
        _slots.FirstOrDefault(s => s.ItemId == itemId)?.Quantity ?? 0;

    public bool Add(string itemId, int quantity = 1)
    {
        if (quantity <= 0) return false;

        var existing = _slots.FirstOrDefault(s => s.ItemId == itemId);
        if (existing is not null)
        {
            existing.Add(quantity);
            return true;
        }

        if (TotalSlotCount() >= Capacity) return false;
        _slots.Add(new ItemStack(itemId, quantity));
        return true;
    }

    public bool Remove(string itemId, int quantity = 1)
    {
        if (quantity <= 0) return false;

        var existing = _slots.FirstOrDefault(s => s.ItemId == itemId);
        if (existing is null || existing.Quantity < quantity) return false;

        existing.Add(-quantity);
        if (existing.Quantity <= 0)
            _slots.Remove(existing);

        return true;
    }

    public IReadOnlyList<ItemStackSave> ToSaveData() =>
        _slots.Select(s => new ItemStackSave(s.ItemId, s.Quantity)).ToList();

    public void Restore(IEnumerable<ItemStackSave> items)
    {
        _slots.Clear();
        foreach (var item in items)
            Add(item.ItemId, item.Quantity);
    }

    public string Describe(Func<string, string>? nameResolver = null)
    {
        if (_slots.Count == 0) return "(empty)";
        nameResolver ??= id => id;
        return string.Join(", ", _slots.Select(s => $"{nameResolver(s.ItemId)} x{s.Quantity}"));
    }

    private int TotalSlotCount() => _slots.Count;
}

public sealed record ItemStackSave(string ItemId, int Quantity);
