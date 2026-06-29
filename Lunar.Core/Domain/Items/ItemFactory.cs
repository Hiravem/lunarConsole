using Lunar.Core.Domain.Characters;

namespace Lunar.Core.Domain.Items;

public sealed class ItemFactory
{
    private readonly Dictionary<string, Item> _items = new();

    public ItemFactory() => RegisterDefaults();

    public Item Create(string itemId) =>
        _items.TryGetValue(itemId, out var item)
            ? CloneItem(item)
            : throw new KeyNotFoundException($"Unknown item: {itemId}");

    public bool TryCreate(string itemId, out Item? item)
    {
        if (!_items.TryGetValue(itemId, out var template))
        {
            item = null;
            return false;
        }

        item = CloneItem(template);
        return true;
    }

    public string GetDisplayName(string itemId) =>
        _items.TryGetValue(itemId, out var item) ? item.Name : itemId;

    public IReadOnlyCollection<string> AllIds => _items.Keys;

    private void RegisterDefaults()
    {
        Register(new Consumable("health_potion", "Health Potion", "Restores 40 HP.", healAmount: 40));
        Register(new Weapon("rusty_dagger", "Rusty Dagger", "A worn blade.", new Stats(attack: 2, defense: 0)));
        Register(new Weapon("iron_sword", "Iron Sword", "A reliable sword.", new Stats(attack: 5, defense: 0)));
        Register(new Armor("leather_armor", "Leather Armor", "Basic protection.", new Stats(attack: 0, defense: 3)));
        Register(new Ring("copper_ring", "Copper Ring", "A simple ring.", new Stats(attack: 2, defense: 1)));
    }

    private void Register(Item item) => _items[item.Id] = item;

    private static Item CloneItem(Item template) => template switch
    {
        Consumable c => new Consumable(c.Id, c.Name, c.Description, c.HealAmount),
        Weapon w => new Weapon(w.Id, w.Name, w.Description, w.StatModifier),
        Armor a => new Armor(a.Id, a.Name, a.Description, a.StatModifier),
        Ring r => new Ring(r.Id, r.Name, r.Description, r.StatModifier),
        _ => throw new InvalidOperationException($"Unsupported item type: {template.Id}")
    };
}
