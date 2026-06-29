using Lunar.Core.Domain.Characters;
using Lunar.Core.Domain.Combat;

namespace Lunar.Core.Application;

public sealed class DayFlags
{
    public bool HasExplored { get; set; }
    public bool HasRested { get; set; }
    public bool HasUsedDayAction => HasExplored || HasRested;

    public void Reset()
    {
        HasExplored = false;
        HasRested = false;
    }
}

public sealed class GameSession
{
    public Player Player { get; set; } = null!;
    public int CurrentDay { get; set; } = 1;
    public int Difficulty { get; set; } = 1;
    public DayFlags DayFlags { get; } = new();
    public bool IsBossDay { get; set; }
    public CombatSession? ActiveCombatSession { get; set; }
    public MetaProgression MetaProgression { get; set; } = new();

    public int DaysUntilBoss =>
        GameBalance.BossIntervalDays - (CurrentDay % GameBalance.BossIntervalDays);

    public static GameSession CreateNew(Player player) =>
        new() { Player = player, CurrentDay = 1, Difficulty = 1, IsBossDay = false };
}
