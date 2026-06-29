using Lunar.Core.Domain.Characters;
using Lunar.Core.Domain.Items;

namespace Lunar.Core.Domain.Inventory;

public sealed class EquipResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? UnequippedItemId { get; init; }
    public EquipmentSlot Slot { get; init; }

    public static EquipResult Ok(EquipmentSlot slot, string? unequippedItemId) =>
        new() { Success = true, Slot = slot, UnequippedItemId = unequippedItemId };

    public static EquipResult Fail(string error) =>
        new() { Success = false, Error = error };
}

public sealed class EquipmentLoadout
{
    public string? WeaponId { get; private set; }
    public string? ArmorId { get; private set; }
    public string? RingId { get; private set; }

    public EquipResult Equip(Item item)
    {
        if (item.EquipSlot is not { } slot)
            return EquipResult.Fail($"{item.Name} cannot be equipped.");

        string? unequipped = slot switch
        {
            EquipmentSlot.Weapon => Set(ref _weaponId, item.Id),
            EquipmentSlot.Armor => Set(ref _armorId, item.Id),
            EquipmentSlot.Ring => Set(ref _ringId, item.Id),
            _ => null
        };

        WeaponId = _weaponId;
        ArmorId = _armorId;
        RingId = _ringId;

        return EquipResult.Ok(slot, unequipped);
    }

    public string? Unequip(EquipmentSlot slot)
    {
        string? removed = slot switch
        {
            EquipmentSlot.Weapon => Clear(ref _weaponId),
            EquipmentSlot.Armor => Clear(ref _armorId),
            EquipmentSlot.Ring => Clear(ref _ringId),
            _ => null
        };

        WeaponId = _weaponId;
        ArmorId = _armorId;
        RingId = _ringId;
        return removed;
    }

    public Stats GetStatsModifier(ItemFactory factory)
    {
        var total = Stats.Zero;
        if (WeaponId is not null && factory.TryCreate(WeaponId, out var weapon))
            total = total.ApplyModifier(weapon!.StatModifier);
        if (ArmorId is not null && factory.TryCreate(ArmorId, out var armor))
            total = total.ApplyModifier(armor!.StatModifier);
        if (RingId is not null && factory.TryCreate(RingId, out var ring))
            total = total.ApplyModifier(ring!.StatModifier);
        return total;
    }

    public EquipmentSaveData ToSaveData() => new(WeaponId, ArmorId, RingId);

    public void Restore(EquipmentSaveData data)
    {
        _weaponId = data.WeaponId;
        _armorId = data.ArmorId;
        _ringId = data.RingId;
        WeaponId = _weaponId;
        ArmorId = _armorId;
        RingId = _ringId;
    }

    private string? _weaponId;
    private string? _armorId;
    private string? _ringId;

    private static string? Set(ref string? field, string itemId)
    {
        var old = field;
        field = itemId;
        return old;
    }

    private static string? Clear(ref string? field)
    {
        var old = field;
        field = null;
        return old;
    }
}

public sealed record EquipmentSaveData(string? WeaponId, string? ArmorId, string? RingId);
