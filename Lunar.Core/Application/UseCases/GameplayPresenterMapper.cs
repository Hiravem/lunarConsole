using Lunar.Core.Application.DTOs;
using Lunar.Core.Domain.Characters;
using Lunar.Core.Domain.Inventory;
using Lunar.Core.Domain.Items;

namespace Lunar.Core.Application.UseCases;

public static class GameplayPresenterMapper
{
    public static GameplayMenuDto ToMenuDto(GameSession session)
    {
        var stats = session.Player.GetEffectiveStats();
        var factory = session.Player.ItemFactory;
        var daysUntil = session.CurrentDay % GameBalance.BossIntervalDays == 0
            ? 0
            : GameBalance.BossIntervalDays -
              (session.CurrentDay % GameBalance.BossIntervalDays);

        return new GameplayMenuDto
        {
            Day = session.CurrentDay,
            HpCurrent = session.Player.Health.Current,
            HpMax = session.Player.Health.Max,
            Attack = stats.Attack,
            Defense = stats.Defense,
            Gold = session.Player.Gold,
            DaysUntilBoss = daysUntil,
            IsBossDay = session.IsBossDay,
            HasExplored = session.DayFlags.HasExplored,
            HasRested = session.DayFlags.HasRested,
            InventorySummary = session.Player.Inventory.Describe(factory.GetDisplayName),
            EquipmentSummary = DescribeEquipment(session.Player.Equipment, factory),
            BossesDefeated = session.MetaProgression.BossesDefeated,
            Difficulty = session.Difficulty
        };
    }

    public static IReadOnlyList<InventoryItemDto> ToInventoryDto(Player player)
    {
        var factory = player.ItemFactory;
        var list = new List<InventoryItemDto>();
        var index = 1;

        foreach (var stack in player.Inventory.Items)
        {
            factory.TryCreate(stack.ItemId, out var item);
            list.Add(new InventoryItemDto
            {
                Index = index++,
                ItemId = stack.ItemId,
                DisplayName = factory.GetDisplayName(stack.ItemId),
                Quantity = stack.Quantity,
                CanEquip = item?.EquipSlot is not null,
                CanUse = item is Consumable
            });
        }

        return list;
    }

    public static EquipmentDto ToEquipmentDto(Player player)
    {
        var factory = player.ItemFactory;
        var equip = player.Equipment;
        var stats = player.GetEffectiveStats();

        return new EquipmentDto
        {
            WeaponName = equip.WeaponId is null ? "(none)" : factory.GetDisplayName(equip.WeaponId),
            ArmorName = equip.ArmorId is null ? "(none)" : factory.GetDisplayName(equip.ArmorId),
            RingName = equip.RingId is null ? "(none)" : factory.GetDisplayName(equip.RingId),
            EffectiveAttack = stats.Attack,
            EffectiveDefense = stats.Defense
        };
    }

    private static string DescribeEquipment(EquipmentLoadout equip, ItemFactory factory)
    {
        var weapon = equip.WeaponId is null ? "-" : factory.GetDisplayName(equip.WeaponId);
        var armor = equip.ArmorId is null ? "-" : factory.GetDisplayName(equip.ArmorId);
        var ring = equip.RingId is null ? "-" : factory.GetDisplayName(equip.RingId);
        return $"W:{weapon} A:{armor} R:{ring}";
    }
}
