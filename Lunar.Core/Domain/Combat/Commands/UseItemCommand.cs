using Lunar.Core.Domain.Characters;
using Lunar.Core.Domain.Items;

namespace Lunar.Core.Domain.Combat.Commands;

public sealed class UseItemCommand : ICombatCommand
{
    private readonly string _itemId;
    private readonly ItemFactory _itemFactory;

    public UseItemCommand(string itemId, ItemFactory itemFactory)
    {
        _itemId = itemId;
        _itemFactory = itemFactory;
    }

    public CommandResult Execute(CombatSession session)
    {
        var player = session.Player;

        if (player.Inventory.GetQuantity(_itemId) <= 0)
            return CommandResult.Fail("Item not in inventory.");

        if (!_itemFactory.TryCreate(_itemId, out var item) || item is not Consumable)
            return CommandResult.Fail("Only consumables can be used in combat.");

        var useResult = item!.Use(player);
        if (!useResult.Success)
            return CommandResult.Fail(useResult.Message);

        if (useResult.Consumed)
            player.Inventory.Remove(_itemId);

        return CommandResult.Ok(useResult.Message);
    }
}
