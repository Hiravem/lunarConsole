using Lunar.Core.Model.Characters;

namespace Lunar.Core.Model.Items;

public sealed class Weapon : Item
{
    private readonly Stats _modifier;

    public Weapon(string id, string name, string description, Stats modifier)
        : base(id, name, description) => _modifier = modifier;

    public override Stats StatModifier => _modifier;
    public override EquipmentSlot? EquipSlot => EquipmentSlot.Weapon;

    public override bool CanEquip(EquipmentSlot slot) => slot == EquipmentSlot.Weapon;

    public override ItemUseResult Use(Player player) =>
        ItemUseResult.Fail("Weapons must be equipped, not used.");
}

public sealed class Armor : Item
{
    private readonly Stats _modifier;

    public Armor(string id, string name, string description, Stats modifier)
        : base(id, name, description) => _modifier = modifier;

    public override Stats StatModifier => _modifier;
    public override EquipmentSlot? EquipSlot => EquipmentSlot.Armor;

    public override bool CanEquip(EquipmentSlot slot) => slot == EquipmentSlot.Armor;

    public override ItemUseResult Use(Player player) =>
        ItemUseResult.Fail("Armor must be equipped, not used.");
}

public sealed class Ring : Item
{
    private readonly Stats _modifier;

    public Ring(string id, string name, string description, Stats modifier)
        : base(id, name, description) => _modifier = modifier;

    public override Stats StatModifier => _modifier;
    public override EquipmentSlot? EquipSlot => EquipmentSlot.Ring;

    public override bool CanEquip(EquipmentSlot slot) => slot == EquipmentSlot.Ring;

    public override ItemUseResult Use(Player player) =>
        ItemUseResult.Fail("Rings must be equipped, not used.");
}

public sealed class Consumable : Item
{
    public int HealAmount { get; }

    public Consumable(string id, string name, string description, int healAmount)
        : base(id, name, description) => HealAmount = healAmount;

    public override bool CanEquip(EquipmentSlot slot) => false;

    public override ItemUseResult Use(Player player)
    {
        if (player.Health.IsDead)
            return ItemUseResult.Fail("Cannot use items while dead.");

        var healed = player.Health.Heal(HealAmount);
        return ItemUseResult.Ok($"Used {Name}. Healed {healed} HP.", consumed: true);
    }
}
