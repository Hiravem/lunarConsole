namespace Lunar.Core.Domain.Items;

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Ring
}

public sealed class ItemUseResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public bool Consumed { get; init; }

    public static ItemUseResult Ok(string message, bool consumed = true) =>
        new() { Success = true, Message = message, Consumed = consumed };

    public static ItemUseResult Fail(string message) =>
        new() { Success = false, Message = message, Consumed = false };
}

public abstract class Item
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    protected Item(string id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public abstract ItemUseResult Use(Characters.Player player);
    public abstract bool CanEquip(EquipmentSlot slot);
    public virtual Characters.Stats StatModifier => Characters.Stats.Zero;
    public virtual EquipmentSlot? EquipSlot => null;
}
