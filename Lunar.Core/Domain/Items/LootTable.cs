namespace Lunar.Core.Domain.Items;

public sealed class LootTable
{
    private readonly (string itemId, int weight)[] _entries;

    public LootTable(params (string itemId, int weight)[] entries) => _entries = entries;

    public static LootTable DefaultEnemy { get; } = new(
        ("health_potion", 60),
        ("rusty_dagger", 25),
        ("iron_sword", 10),
        ("leather_armor", 5));

    public static LootTable Goblin { get; } = new(
        ("health_potion", 70),
        ("rusty_dagger", 30));

    public static LootTable Skeleton { get; } = new(
        ("health_potion", 50),
        ("iron_sword", 30),
        ("leather_armor", 20));

    public static LootTable Boss { get; } = new(
        ("health_potion", 40),
        ("iron_sword", 35),
        ("leather_armor", 25));

    public static LootTable Chest { get; } = new(
        ("health_potion", 40),
        ("rusty_dagger", 30),
        ("iron_sword", 20),
        ("copper_ring", 10));

    public static LootTable Resolve(string lootTableId) => lootTableId switch
    {
        "goblin_loot" => Goblin,
        "skeleton_loot" => Skeleton,
        "boss_loot" => Boss,
        "chest_loot" => Chest,
        _ => DefaultEnemy
    };

    public string Roll(Application.Interfaces.IRandomService random)
    {
        var total = _entries.Sum(e => e.weight);
        var roll = random.Next(total);
        var cumulative = 0;

        foreach (var (itemId, weight) in _entries)
        {
            cumulative += weight;
            if (roll < cumulative)
                return itemId;
        }

        return _entries[0].itemId;
    }
}
