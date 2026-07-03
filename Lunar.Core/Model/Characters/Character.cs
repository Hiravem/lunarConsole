using Lunar.Core.Model.Combat;
using Lunar.Core.Model.Inventory;
using Lunar.Core.Model.Items;
using Lunar.Core.Model.Skills;

namespace Lunar.Core.Model.Characters;

public abstract class Character : IDamageable
{
    public string Name { get; }
    public Health Health { get; protected set; }
    public Stats Stats { get; protected set; }

    protected Character(string name, Health health, Stats stats)
    {
        Name = name;
        Health = health;
        Stats = stats;
    }

    public bool IsAlive => !Health.IsDead;

    public virtual int TakeDamage(Damage damage) => Health.TakeDamage(damage.Amount);
}

public sealed class Player : Character
{
    private readonly ItemFactory _itemFactory;

    public Model.Inventory.Inventory Inventory { get; } = new();
    public EquipmentLoadout Equipment { get; } = new();
    public PlayerSkillState SkillState { get; } = new();
    public int Gold { get; private set; }

    public Player(string name, Health health, Stats stats, ItemFactory itemFactory, int gold = 0)
        : base(name, health, stats)
    {
        _itemFactory = itemFactory;
        Gold = gold;
    }

    public Stats GetEffectiveStats() =>
        Stats.ApplyModifier(Equipment.GetStatsModifier(_itemFactory));

    public int Rest()
    {
        var healAmount = (int)Math.Ceiling(Health.Max * GameBalance.RestHealPercent);
        return Health.Heal(healAmount);
    }

    public void AddGold(int amount) => Gold += amount;

    public bool TrySpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        return true;
    }

    public ItemFactory ItemFactory => _itemFactory;
}

public class Enemy : Character
{
    public string Id { get; }
    public string LootTableId { get; }
    public IEnemyAI AI { get; protected set; }

    public Enemy(string id, string name, Health health, Stats stats, string lootTableId, IEnemyAI ai)
        : base(name, health, stats)
    {
        Id = id;
        LootTableId = lootTableId;
        AI = ai;
    }

    public bool IsBoss => this is Bosses.Boss;
}

public static class GameBalance
{
    public const double RestHealPercent = 0.30;
    public const int BossIntervalDays = 5;
    public const string DefaultLootItemId = "health_potion";
}
