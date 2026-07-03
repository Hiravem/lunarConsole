using Lunar.Core.Model.Combat;
using Lunar.Core.Model.Combat.Commands;
using Lunar.Core.Model.Dto;
using Lunar.Core.Model.Events;
using Lunar.Core.Model.Items;
using Lunar.Core.Model.Skills;
using Lunar.Core.Service;

namespace Lunar.Core.Tests;

public class Sprint2Tests
{
    private readonly ItemFactory _items = new();

    [Fact]
    public void ItemFactory_CreatesAllItemTypes()
    {
        Assert.IsType<Consumable>(_items.Create("health_potion"));
        Assert.IsType<Weapon>(_items.Create("iron_sword"));
        Assert.IsType<Armor>(_items.Create("leather_armor"));
        Assert.IsType<Ring>(_items.Create("copper_ring"));
    }

    [Fact]
    public void EquipItemUseCase_EquipsWeaponAndSwaps()
    {
        var session = TestHelpers.CreateSession(_items);
        session.Player.Inventory.Add("rusty_dagger");
        session.Player.Inventory.Add("iron_sword");
        var bus = new InMemoryEventBus();

        var first = new EquipItemUseCase(_items, bus).Execute(session, "rusty_dagger");
        Assert.True(first.Success);
        Assert.Null(first.SwapMessage);
        Assert.Equal(17, session.Player.GetEffectiveStats().Attack); // 15+2

        var second = new EquipItemUseCase(_items, bus).Execute(session, "iron_sword");
        Assert.True(second.Success);
        Assert.NotNull(second.SwapMessage);
        Assert.Equal(20, session.Player.GetEffectiveStats().Attack); // 15+5
        Assert.Equal(1, session.Player.Inventory.GetQuantity("rusty_dagger"));
    }

    [Fact]
    public void UseItemUseCase_HealsOutsideCombat()
    {
        var session = TestHelpers.CreateSession(_items);
        session.Player.Inventory.Add("health_potion");
        session.Player.Health.TakeDamage(50);

        var result = new UseItemUseCase(_items).Execute(session, "health_potion");

        Assert.True(result.Success);
        Assert.True(session.Player.Health.Current > 70);
        Assert.Equal(0, session.Player.Inventory.GetQuantity("health_potion"));
    }

    [Fact]
    public void UseItemCommand_WorksInCombat()
    {
        var session = TestHelpers.CreateSession(_items);
        session.Player.Inventory.Add("health_potion");
        session.Player.Health.TakeDamage(40);
        var enemy = new Model.World.EnemyFactory().Create("goblin", 1);
        var combat = new CombatSession(session.Player, enemy);

        var result = combat.Execute(new UseItemCommand("health_potion", _items));

        Assert.True(result.Success);
        Assert.Equal(0, session.Player.Inventory.GetQuantity("health_potion"));
    }

    [Fact]
    public void SkillCommand_HasCooldown()
    {
        var session = TestHelpers.CreateSession(_items);
        var enemy = new Model.World.EnemyFactory().Create("goblin", 1);
        var combat = new CombatSession(session.Player, enemy);
        var skill = new SkillCommand(SkillDefinition.HeroStrike);

        var first = combat.Execute(skill);
        Assert.True(first.Success);

        var second = combat.Execute(skill);
        Assert.False(second.Success);
        Assert.Contains("cooldown", second.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ApplyLootUseCase_GrantsItemFromTable()
    {
        var session = TestHelpers.CreateSession(_items);
        var bus = new InMemoryEventBus();
        var applyLoot = new ApplyLootUseCase(new FixedRandomService(0), bus, _items);

        var message = applyLoot.ApplyFromLootTable(session, "goblin_loot");

        Assert.Contains("Loot obtained", message);
        Assert.True(session.Player.Inventory.Items.Count > 0);
        Assert.Contains(bus.Published, e => e is LootGranted);
    }
}
