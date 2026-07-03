using Lunar.Core.Model;
using Lunar.Core.Model.Dto;
using Lunar.Core.Util;
using Lunar.Core.Model.Events;
using Lunar.Core.Model.Items;

namespace Lunar.Core.Service;

public sealed class EquipItemUseCase
{
    private readonly ItemFactory _itemFactory;
    private readonly IEventBus _eventBus;

    public EquipItemUseCase(ItemFactory itemFactory, IEventBus eventBus)
    {
        _itemFactory = itemFactory;
        _eventBus = eventBus;
    }

    public EquipResultDto Execute(GameSession session, string itemId)
    {
        var player = session.Player;

        if (player.Inventory.GetQuantity(itemId) <= 0)
            return EquipResultDto.Fail("Item not in inventory.");

        if (!_itemFactory.TryCreate(itemId, out var item))
            return EquipResultDto.Fail("Unknown item.");

        if (item!.EquipSlot is null)
            return EquipResultDto.Fail($"{item.Name} cannot be equipped.");

        if (!player.Inventory.Remove(itemId))
            return EquipResultDto.Fail("Failed to remove item from inventory.");

        var equipResult = player.Equipment.Equip(item);

        if (!equipResult.Success)
        {
            player.Inventory.Add(itemId);
            return EquipResultDto.Fail(equipResult.Error ?? "Equip failed.");
        }

        if (equipResult.UnequippedItemId is not null)
        {
            if (!player.Inventory.Add(equipResult.UnequippedItemId))
            {
                player.Equipment.Unequip(equipResult.Slot);
                player.Inventory.Add(itemId);
                return EquipResultDto.Fail("Inventory full — cannot swap equipment.");
            }
        }

        _eventBus.Publish(new ItemEquipped(itemId, equipResult.Slot.ToString()));

        return EquipResultDto.Ok(
            $"Equipped {_itemFactory.GetDisplayName(itemId)}.",
            equipResult.UnequippedItemId is not null
                ? $"Unequipped {_itemFactory.GetDisplayName(equipResult.UnequippedItemId)}."
                : null);
    }
}
