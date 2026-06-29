using Lunar.Core.Application.DTOs;
using Lunar.Core.Domain.Items;

namespace Lunar.Core.Application.UseCases;

public sealed class UseItemUseCase
{
    private readonly ItemFactory _itemFactory;

    public UseItemUseCase(ItemFactory itemFactory) => _itemFactory = itemFactory;

    public UseItemResultDto Execute(GameSession session, string itemId)
    {
        if (session.ActiveCombatSession is not null)
            return UseItemResultDto.Fail("Use items from the combat menu during battle.");

        var player = session.Player;

        if (player.Inventory.GetQuantity(itemId) <= 0)
            return UseItemResultDto.Fail("Item not in inventory.");

        if (!_itemFactory.TryCreate(itemId, out var item))
            return UseItemResultDto.Fail("Unknown item.");

        if (item is not Consumable)
            return UseItemResultDto.Fail("Only consumables can be used from inventory.");

        var useResult = item.Use(player);
        if (!useResult.Success)
            return UseItemResultDto.Fail(useResult.Message);

        if (useResult.Consumed)
            player.Inventory.Remove(itemId);

        return new UseItemResultDto
        {
            Success = true,
            Message = useResult.Message
        };
    }
}
