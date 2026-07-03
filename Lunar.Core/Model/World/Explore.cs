using Lunar.Core.Util;
using Lunar.Core.Model.Characters;
using Lunar.Core.Model.Combat;

namespace Lunar.Core.Model.World;

public interface IExploreEncounter
{
    string Id { get; }
    string Description { get; }
    EncounterResult Resolve(ExploreContext context);
}

public sealed class ExploreContext
{
    public int Difficulty { get; init; }
    public IRandomService Random { get; init; } = null!;
}

public enum EncounterResultType
{
    Combat,
    Narrative,
    Loot,
    Merchant,
    Event
}

public sealed class EncounterResult
{
    public EncounterResultType Type { get; init; }
    public string Message { get; init; } = "";
    public string? EnemyId { get; init; }
    public string? LootTableId { get; init; }
    public int? GoldChange { get; init; }
    public int? HealAmount { get; init; }
    public int? DamageAmount { get; init; }

    public bool StartsCombat => Type == EncounterResultType.Combat;
    public bool OpensMerchant => Type == EncounterResultType.Merchant;

    public static EncounterResult Combat(string enemyId, string message) =>
        new() { Type = EncounterResultType.Combat, EnemyId = enemyId, Message = message };

    public static EncounterResult Narrative(string message) =>
        new() { Type = EncounterResultType.Narrative, Message = message };

    public static EncounterResult Loot(string lootTableId, string message) =>
        new() { Type = EncounterResultType.Loot, LootTableId = lootTableId, Message = message };

    public static EncounterResult Merchant(string message) =>
        new() { Type = EncounterResultType.Merchant, Message = message };

    public static EncounterResult EventGold(int amount, string message) =>
        new() { Type = EncounterResultType.Event, GoldChange = amount, Message = message };

    public static EncounterResult EventHeal(int amount, string message) =>
        new() { Type = EncounterResultType.Event, HealAmount = amount, Message = message };

    public static EncounterResult EventDamage(int amount, string message) =>
        new() { Type = EncounterResultType.Event, DamageAmount = amount, Message = message };
}

public sealed class EnemyEncounter : IExploreEncounter
{
    public string Id { get; }
    public string EnemyId { get; }
    public string Description { get; }

    public EnemyEncounter(string id, string enemyId, string description)
    {
        Id = id;
        EnemyId = enemyId;
        Description = description;
    }

    public EncounterResult Resolve(ExploreContext context) =>
        EncounterResult.Combat(EnemyId, Description);
}

public sealed class ChestEncounter : IExploreEncounter
{
    public string Id { get; }
    public string LootTableId { get; }
    public string Description { get; }

    public ChestEncounter(string id, string lootTableId, string description)
    {
        Id = id;
        LootTableId = lootTableId;
        Description = description;
    }

    public EncounterResult Resolve(ExploreContext context) =>
        EncounterResult.Loot(LootTableId, Description);
}

public sealed class EmptyEncounter : IExploreEncounter
{
    public string Id { get; }
    public string Description { get; }

    public EmptyEncounter(string id, string description)
    {
        Id = id;
        Description = description;
    }

    public EncounterResult Resolve(ExploreContext context) =>
        EncounterResult.Narrative(Description);
}

public sealed class EventEncounter : IExploreEncounter
{
    public string Id { get; }
    public string Description { get; }

    public EventEncounter(string id, string description)
    {
        Id = id;
        Description = description;
    }

    public EncounterResult Resolve(ExploreContext context)
    {
        var roll = context.Random.Next(100);
        return roll switch
        {
            < 35 => EncounterResult.EventGold(15, "You find a pouch with 15 gold!"),
            < 65 => EncounterResult.EventHeal(25, "A warm shrine restores 25 HP."),
            < 85 => EncounterResult.EventDamage(10, "A trap springs! You take 10 damage."),
            _ => EncounterResult.Narrative("Strange whispers fade into silence. Nothing happens.")
        };
    }
}

public sealed class MerchantEncounter : IExploreEncounter
{
    public string Id { get; }
    public string Description { get; }

    public MerchantEncounter(string id, string description)
    {
        Id = id;
        Description = description;
    }

    public EncounterResult Resolve(ExploreContext context) =>
        EncounterResult.Merchant(Description);
}

public static class ExploreResolver
{
    public static EncounterResult Resolve(IExploreEncounter encounter, ExploreContext context) =>
        encounter.Resolve(context);
}
