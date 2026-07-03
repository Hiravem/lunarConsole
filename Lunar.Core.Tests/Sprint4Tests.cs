using Lunar.Console.Repository;
using Lunar.Core.Model;
using Lunar.Core.Service;
using Lunar.Core.Model.Combat;
using Lunar.Core.Model.Items;

namespace Lunar.Core.Tests;

public class Sprint4Tests
{
    private readonly ItemFactory _items = new();
    private readonly string _tempSavePath;

    public Sprint4Tests()
    {
        _tempSavePath = Path.Combine(Path.GetTempPath(), $"lunar_test_{Guid.NewGuid():N}.json");
    }

    [Fact]
    public void GameState_RoundTrip_PreservesSession()
    {
        var original = TestHelpers.CreateSession(_items);
        original.CurrentDay = 3;
        original.Difficulty = 2;
        original.Player.Inventory.Add("iron_sword");
        original.Player.AddGold(50);
        original.DayFlags.HasExplored = true;
        original.MetaProgression.BossesDefeated = 1;

        new EquipItemUseCase(_items, new InMemoryEventBus())
            .Execute(original, "iron_sword");

        var state = GameState.FromSession(original);
        var restored = state.ToSession(_items);

        Assert.Equal(3, restored.CurrentDay);
        Assert.Equal(2, restored.Difficulty);
        Assert.Equal(50, restored.Player.Gold);
        Assert.Equal(1, restored.MetaProgression.BossesDefeated);
        Assert.True(restored.DayFlags.HasExplored);
        Assert.Equal("iron_sword", restored.Player.Equipment.WeaponId);
        Assert.Equal(20, restored.Player.GetEffectiveStats().Attack);
    }

    [Fact]
    public void JsonSaveRepository_SaveAndLoad()
    {
        var repo = new JsonSaveRepository(_tempSavePath);
        var session = TestHelpers.CreateSession(_items);
        session.Player.Inventory.Add("health_potion", 3);
        session.CurrentDay = 4;

        repo.Save(GameState.FromSession(session));

        Assert.True(repo.HasSave());

        var loaded = repo.Load();
        Assert.NotNull(loaded);
        Assert.Equal(4, loaded!.CurrentDay);
        Assert.Single(loaded.Inventory);
        Assert.Equal("health_potion", loaded.Inventory[0].ItemId);
        Assert.Equal(3, loaded.Inventory[0].Quantity);
    }

    [Fact]
    public void SaveGameUseCase_SavesSuccessfully()
    {
        var repo = new JsonSaveRepository(_tempSavePath);
        var session = TestHelpers.CreateSession(_items);
        session.CurrentDay = 2;
        var bus = new InMemoryEventBus();

        var result = new SaveGameUseCase(repo, bus).Execute(session);

        Assert.True(result.Success);
        Assert.True(repo.HasSave());
        Assert.Contains(bus.Published, e => e is Model.Events.GameSaved);
    }

    [Fact]
    public void SaveGameUseCase_FailsDuringCombat()
    {
        var repo = new JsonSaveRepository(_tempSavePath);
        var session = TestHelpers.CreateSession(_items);
        var enemy = new Model.World.EnemyFactory().Create("goblin", 1);
        session.ActiveCombatSession = new CombatSession(session.Player, enemy);

        var result = new SaveGameUseCase(repo, new InMemoryEventBus()).Execute(session);

        Assert.False(result.Success);
        Assert.False(repo.HasSave());
    }

    [Fact]
    public void LoadGameUseCase_RestoresSession()
    {
        var repo = new JsonSaveRepository(_tempSavePath);
        var session = TestHelpers.CreateSession(_items);
        session.CurrentDay = 5;
        session.Difficulty = 2;
        repo.Save(GameState.FromSession(session));

        var result = new LoadGameUseCase(repo, _items).Execute();

        Assert.True(result.Success);
        Assert.NotNull(result.Session);
        Assert.Equal(5, result.Session!.CurrentDay);
        Assert.Equal(2, result.Session.Difficulty);
    }

    [Fact]
    public void LoadGameUseCase_FailsWhenNoSave()
    {
        var repo = new JsonSaveRepository(_tempSavePath);
        if (File.Exists(_tempSavePath)) File.Delete(_tempSavePath);

        var result = new LoadGameUseCase(repo, _items).Execute();

        Assert.False(result.Success);
    }

    [Fact]
    public void JsonSaveRepository_DeleteSave()
    {
        var repo = new JsonSaveRepository(_tempSavePath);
        repo.Save(GameState.FromSession(TestHelpers.CreateSession(_items)));
        Assert.True(repo.HasSave());

        repo.DeleteSave();
        Assert.False(repo.HasSave());
    }
}
