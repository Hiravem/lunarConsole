using Lunar.Core.Application.Interfaces;
using Lunar.Core.Domain.Characters;
using Lunar.Core.Domain.Events;
using Lunar.Core.Domain.Items;

namespace Lunar.Core.Application.UseCases;

public sealed class ApplyLootUseCase
{
    private readonly IRandomService _random;
    private readonly IEventBus _eventBus;
    private readonly ItemFactory _itemFactory;

    public ApplyLootUseCase(IRandomService random, IEventBus eventBus, ItemFactory itemFactory)
    {
        _random = random;
        _eventBus = eventBus;
        _itemFactory = itemFactory;
    }

    public string ApplyFromLootTable(GameSession session, string lootTableId)
    {
        var table = LootTable.Resolve(lootTableId);
        var itemId = table.Roll(_random);
        return ApplyItem(session, itemId);
    }

    public string ApplyFromChest(GameSession session, string lootTableId)
    {
        _eventBus.Publish(new ChestOpened(lootTableId));
        return ApplyFromLootTable(session, lootTableId);
    }

    public string ApplyItem(GameSession session, string itemId)
    {
        if (!session.Player.Inventory.Add(itemId))
            return $"Loot: {_itemFactory.GetDisplayName(itemId)} — inventory full!";

        _eventBus.Publish(new LootGranted(new[] { itemId }));
        return $"Loot obtained: {_itemFactory.GetDisplayName(itemId)}";
    }

    public string ApplyBossRewards(GameSession session, Enemy enemy)
    {
        var lootMessage = ApplyFromLootTable(session, enemy.LootTableId);
        _eventBus.Publish(new BossDefeated(enemy.Id, enemy.LootTableId));
        session.Difficulty++;
        session.MetaProgression.OnBossDefeated(session.Difficulty);
        session.CurrentDay = 1;
        session.IsBossDay = false;
        session.DayFlags.Reset();
        return $"Boss defeated! {lootMessage} Difficulty increased to {session.Difficulty}. New cycle begins.";
    }
}
