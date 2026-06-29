using Lunar.Core.Application.UseCases;
using Lunar.Core.Domain.Bosses;
using Lunar.Core.Domain.Combat;
using Lunar.Core.Domain.Combat.Commands;
using Lunar.Core.Domain.Items;
using Lunar.Core.Domain.World;

namespace Lunar.Core.Tests;

public class Sprint3Tests
{
    private readonly ItemFactory _items = new();

    [Fact]
    public void Boss_CheckPhaseTransition_AtHpThresholds()
    {
        var boss = new BossFactory().Create("skeleton_king", 1);
        var session = TestHelpers.CreateSession(_items);
        var combat = new CombatSession(session.Player, boss);

        // Drop to phase 2 (below 66%)
        boss.Health.TakeDamage((int)(boss.Health.Max * 0.35));
        boss.CheckPhaseTransition(combat);

        Assert.Equal(2, boss.PhaseNumber);
        Assert.Equal("phase_2", boss.CurrentPhase.PhaseId);

        // Drop to enraged (below 33%)
        boss.Health.TakeDamage((int)(boss.Health.Max * 0.40));
        boss.CheckPhaseTransition(combat);

        Assert.Equal(3, boss.PhaseNumber);
        Assert.Equal("phase_enraged", boss.CurrentPhase.PhaseId);
    }

    [Fact]
    public void ExploreUseCase_ChestEncounter_GrantsLoot()
    {
        var session = TestHelpers.CreateSession(_items);
        var bus = new InMemoryEventBus();
        var applyLoot = new ApplyLootUseCase(new FixedRandomService(0), bus, _items);

        // goblin=35, skeleton=25, chest=15 -> roll 60 = chest
        var table = new EncounterTable(
            (new EnemyEncounter("e1", "goblin", "goblin"), 35),
            (new EnemyEncounter("e2", "skeleton", "sk"), 25),
            (new ChestEncounter("chest", "chest_loot", "A chest!"), 15));

        var explore = new ExploreUseCase(new FixedRandomService(60), applyLoot, table);
        var result = explore.Execute(session);

        Assert.True(result.Success);
        Assert.Equal(Application.DTOs.ExploreEncounterType.Loot, result.EncounterType);
        Assert.NotNull(result.EffectMessage);
        Assert.Contains(bus.Published, e => e is Domain.Events.ChestOpened);
    }

    [Fact]
    public void ExploreUseCase_EventEncounter_GrantsGold()
    {
        var session = TestHelpers.CreateSession(_items);
        var bus = new InMemoryEventBus();
        var applyLoot = new ApplyLootUseCase(new FixedRandomService(0), bus, _items);
        var explore = new ExploreUseCase(new FixedRandomService(0), applyLoot);

        // Force event encounter: goblin+skeleton=60, chest=15, empty=10 -> roll 75 = event
        var table = new EncounterTable(
            (new EnemyEncounter("e1", "goblin", "g"), 35),
            (new EnemyEncounter("e2", "skeleton", "s"), 25),
            (new ChestEncounter("c", "chest_loot", "c"), 15),
            (new EmptyEncounter("em", "quiet"), 10),
            (new EventEncounter("ev", "shrine"), 10));

        explore = new ExploreUseCase(new FixedRandomService(88), applyLoot, table);
        var result = explore.Execute(session);

        Assert.True(result.Success);
        Assert.Equal(Application.DTOs.ExploreEncounterType.Event, result.EncounterType);
    }

    [Fact]
    public void ExploreUseCase_MerchantEncounter_OpensShop()
    {
        var session = TestHelpers.CreateSession(_items);
        var bus = new InMemoryEventBus();
        var applyLoot = new ApplyLootUseCase(new FixedRandomService(0), bus, _items);

        // roll 95 = merchant (last 5% weight)
        var table = new EncounterTable(
            (new EnemyEncounter("e1", "goblin", "g"), 35),
            (new EnemyEncounter("e2", "skeleton", "s"), 25),
            (new ChestEncounter("c", "chest_loot", "c"), 15),
            (new EmptyEncounter("em", "quiet"), 10),
            (new EventEncounter("ev", "shrine"), 10),
            (new MerchantEncounter("m", "merchant"), 5));

        var explore = new ExploreUseCase(new FixedRandomService(95), applyLoot, table);
        var result = explore.Execute(session);

        Assert.True(result.Success);
        Assert.True(result.OpensMerchant);
    }

    [Fact]
    public void MerchantUseCase_BuyAndSell()
    {
        var session = TestHelpers.CreateSession(_items);
        session.Player.AddGold(100);
        var merchant = new MerchantUseCase(_items);

        var buy = merchant.Buy(session, "health_potion");
        Assert.True(buy.Success);
        Assert.Equal(1, session.Player.Inventory.GetQuantity("health_potion"));
        Assert.Equal(80, session.Player.Gold); // 100 - 20

        var sell = merchant.Sell(session, "health_potion");
        Assert.True(sell.Success);
        Assert.Equal(0, session.Player.Inventory.GetQuantity("health_potion"));
        Assert.Equal(90, session.Player.Gold); // 80 + 10
    }

    [Fact]
    public void BossFactory_CreatesSkeletonKing()
    {
        var boss = new BossFactory().Create("skeleton_king", 1);
        Assert.Equal("skeleton_king", boss.Id);
        Assert.Equal("Skeleton King", boss.Name);
        Assert.True(boss.IsBoss);
    }

    [Fact]
    public void ApplyLootUseCase_BossDefeat_IncreasesDifficulty()
    {
        var session = TestHelpers.CreateSession(_items);
        var bus = new InMemoryEventBus();
        var applyLoot = new ApplyLootUseCase(new FixedRandomService(0), bus, _items);
        var boss = new BossFactory().Create("skeleton_king", 1);

        var message = applyLoot.ApplyBossRewards(session, boss);

        Assert.Contains("Boss defeated", message);
        Assert.Equal(2, session.Difficulty);
        Assert.Equal(1, session.MetaProgression.BossesDefeated);
        Assert.Equal(1, session.CurrentDay);
    }
}
