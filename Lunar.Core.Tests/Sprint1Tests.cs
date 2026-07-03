using Lunar.Core.Service;
using Lunar.Core.Model.Characters;
using Lunar.Core.Model.Combat;
using Lunar.Core.Model.Combat.Commands;
using Lunar.Core.Model.Items;
using Lunar.Core.Model.World;

namespace Lunar.Core.Tests;

public class Sprint1Tests
{
    private readonly ItemFactory _items = new();

    [Fact]
    public void Health_TakeDamage_And_Heal()
    {
        var health = new Health(100);
        health.TakeDamage(30);
        Assert.Equal(70, health.Current);
        health.Heal(20);
        Assert.Equal(90, health.Current);
    }

    [Fact]
    public void Player_GetEffectiveStats_IncludesEquipment()
    {
        var session = TestHelpers.CreateSession(_items);
        session.Player.Inventory.Add("iron_sword");
        new EquipItemUseCase(_items, new InMemoryEventBus())
            .Execute(session, "iron_sword");

        var stats = session.Player.GetEffectiveStats();
        Assert.Equal(20, stats.Attack); // 15 base + 5 sword
    }

    [Fact]
    public void CombatSession_Attack_DefeatsEnemy()
    {
        var session = TestHelpers.CreateSession(_items);
        var enemy = new EnemyFactory().Create("goblin", 1);
        var combat = new CombatSession(session.Player, enemy);

        while (!combat.IsFinished)
        {
            var result = combat.Execute(new AttackCommand(isPlayer: true));
            Assert.True(result.Success);
        }

        Assert.Equal(CombatWinner.Player, combat.Winner);
        Assert.False(enemy.IsAlive);
    }

    [Fact]
    public void CombatSession_Flee_EndsWithoutLoot()
    {
        var session = TestHelpers.CreateSession(_items);
        var enemy = new EnemyFactory().Create("goblin", 1);
        var combat = new CombatSession(session.Player, enemy);

        var result = combat.Execute(new FleeCommand());

        Assert.True(result.Success);
        Assert.True(combat.Fled);
        Assert.True(combat.IsFinished);
    }

    [Fact]
    public void RestUseCase_HealsPlayer()
    {
        var session = TestHelpers.CreateSession(_items);
        session.Player.Health.TakeDamage(50);

        var result = new RestUseCase().Execute(session);

        Assert.True(result.Success);
        Assert.True(result.HealedAmount > 0);
        Assert.True(session.DayFlags.HasRested);
    }

    [Fact]
    public void AdvanceDayUseCase_RequiresDayAction()
    {
        var session = TestHelpers.CreateSession(_items);
        var bus = new InMemoryEventBus();

        var fail = new AdvanceDayUseCase(bus).Execute(session);
        Assert.False(fail.Success);

        session.DayFlags.HasExplored = true;
        var ok = new AdvanceDayUseCase(bus).Execute(session);
        Assert.True(ok.Success);
        Assert.Equal(2, session.CurrentDay);
    }

    [Fact]
    public void ExploreUseCase_StartsCombat()
    {
        var session = TestHelpers.CreateSession(_items);
        var bus = new InMemoryEventBus();
        var applyLoot = new ApplyLootUseCase(new FixedRandomService(0), bus, _items);
        // weight 0 = first entry = goblin encounter
        var explore = new ExploreUseCase(new FixedRandomService(0), applyLoot);

        var result = explore.Execute(session);

        Assert.True(result.Success);
        Assert.True(result.StartsCombat);
        Assert.Equal("goblin", result.EnemyId);
    }
}
