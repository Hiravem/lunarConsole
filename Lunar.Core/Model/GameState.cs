using Lunar.Core.Model.Characters;
using Lunar.Core.Model.Inventory;
using Lunar.Core.Model.Items;

namespace Lunar.Core.Model;

public sealed class GameState
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;
    public string PlayerName { get; init; } = "Hero";
    public int HpCurrent { get; init; }
    public int HpMax { get; init; }
    public int BaseAttack { get; init; }
    public int BaseDefense { get; init; }
    public int CritChance { get; init; }
    public int Gold { get; init; }
    public int CurrentDay { get; init; }
    public int Difficulty { get; init; }
    public bool IsBossDay { get; init; }
    public bool HasExplored { get; init; }
    public bool HasRested { get; init; }
    public int BossesDefeated { get; init; }
    public int HighestDifficultyReached { get; init; } = 1;
    public List<ItemStackSaveDto> Inventory { get; init; } = new();
    public EquipmentSaveDto Equipment { get; init; } = new();

    public static GameState FromSession(GameSession session) =>
        new()
        {
            SchemaVersion = CurrentSchemaVersion,
            PlayerName = session.Player.Name,
            HpCurrent = session.Player.Health.Current,
            HpMax = session.Player.Health.Max,
            BaseAttack = session.Player.Stats.Attack,
            BaseDefense = session.Player.Stats.Defense,
            CritChance = session.Player.Stats.CritChance,
            Gold = session.Player.Gold,
            CurrentDay = session.CurrentDay,
            Difficulty = session.Difficulty,
            IsBossDay = session.IsBossDay,
            HasExplored = session.DayFlags.HasExplored,
            HasRested = session.DayFlags.HasRested,
            BossesDefeated = session.MetaProgression.BossesDefeated,
            HighestDifficultyReached = session.MetaProgression.HighestDifficultyReached,
            Inventory = session.Player.Inventory.ToSaveData()
                .Select(i => new ItemStackSaveDto(i.ItemId, i.Quantity))
                .ToList(),
            Equipment = new EquipmentSaveDto
            {
                WeaponId = session.Player.Equipment.WeaponId,
                ArmorId = session.Player.Equipment.ArmorId,
                RingId = session.Player.Equipment.RingId
            }
        };

    public GameSession ToSession(ItemFactory itemFactory)
    {
        var session = new GameSession
        {
            Player = new Player(
                PlayerName,
                new Health(HpMax, HpCurrent),
                new Stats(BaseAttack, BaseDefense, CritChance),
                itemFactory,
                Gold),
            CurrentDay = CurrentDay,
            Difficulty = Difficulty,
            IsBossDay = IsBossDay,
            MetaProgression = new MetaProgression
            {
                BossesDefeated = BossesDefeated,
                HighestDifficultyReached = HighestDifficultyReached
            }
        };

        session.Player.Inventory.Restore(Inventory.Select(i => new ItemStackSave(i.ItemId, i.Quantity)));
        session.Player.Equipment.Restore(new EquipmentSaveData(
            Equipment.WeaponId, Equipment.ArmorId, Equipment.RingId));
        session.DayFlags.HasExplored = HasExplored;
        session.DayFlags.HasRested = HasRested;
        session.ActiveCombatSession = null;

        return session;
    }
}

public sealed class ItemStackSaveDto
{
    public string ItemId { get; init; } = "";
    public int Quantity { get; init; }

    public ItemStackSaveDto() { }

    public ItemStackSaveDto(string itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}

public sealed class EquipmentSaveDto
{
    public string? WeaponId { get; init; }
    public string? ArmorId { get; init; }
    public string? RingId { get; init; }
}
