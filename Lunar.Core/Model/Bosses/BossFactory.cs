using Lunar.Core.Exception;
using Lunar.Core.Model.Bosses.Phases;
using Lunar.Core.Model.Characters;

namespace Lunar.Core.Model.Bosses;

public sealed class BossDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public int BaseHp { get; init; }
    public int BaseAttack { get; init; }
    public int BaseDefense { get; init; }
    public int CritChance { get; init; }
    public string LootTableId { get; init; } = "boss_loot";
}

public static class BossDatabase
{
    public static BossDefinition SkeletonKing { get; } = new()
    {
        Id = "skeleton_king",
        Name = "Skeleton King",
        BaseHp = 450,
        BaseAttack = 18,
        BaseDefense = 8,
        CritChance = 5,
        LootTableId = "boss_loot"
    };

    public static BossDefinition? Get(string bossId) => bossId switch
    {
        "skeleton_king" => SkeletonKing,
        _ => null
    };
}

public sealed class BossFactory
{
    public Boss Create(string bossId, int difficulty)
    {
        var def = BossDatabase.Get(bossId)
            ?? throw new EntityNotFoundException("boss", bossId);

        var scale = 1 + (difficulty - 1) * 0.15;

        return new Boss(
            def.Id,
            def.Name,
            new Health((int)(def.BaseHp * scale)),
            new Stats((int)(def.BaseAttack * scale), (int)(def.BaseDefense * scale), def.CritChance),
            def.LootTableId,
            Phase1Behavior.Instance,
            Phase2Behavior.Instance,
            EnragedBehavior.Instance);
    }
}
