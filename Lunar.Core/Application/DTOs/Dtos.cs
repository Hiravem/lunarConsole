namespace Lunar.Core.Application.DTOs;

using Lunar.Core.Application;

public enum ExploreEncounterType
{
    Combat,
    Narrative,
    Loot,
    Merchant,
    Event
}

public sealed class GameplayMenuDto
{
    public int Day { get; init; }
    public int HpCurrent { get; init; }
    public int HpMax { get; init; }
    public int Attack { get; init; }
    public int Defense { get; init; }
    public int Gold { get; init; }
    public int DaysUntilBoss { get; init; }
    public bool IsBossDay { get; init; }
    public bool HasExplored { get; init; }
    public bool HasRested { get; init; }
    public string InventorySummary { get; init; } = "";
    public string EquipmentSummary { get; init; } = "";
    public int BossesDefeated { get; init; }
    public int Difficulty { get; init; }
}

public sealed class ExploreResultDto
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string Message { get; init; } = "";
    public ExploreEncounterType EncounterType { get; init; }
    public bool StartsCombat { get; init; }
    public string? EnemyId { get; init; }
    public bool OpensMerchant { get; init; }
    public string? EffectMessage { get; init; }
}

public sealed class CombatResultDto
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<string> LogLines { get; init; } = Array.Empty<string>();
    public bool CombatEnded { get; init; }
    public bool PlayerWon { get; init; }
    public bool Fled { get; init; }
    public bool PlayerDied { get; init; }
    public string? LootMessage { get; init; }
    public int? BossPhase { get; init; }
}

public sealed class RestResultDto
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public int HealedAmount { get; init; }
}

public sealed class AdvanceDayResultDto
{
    public bool Success { get; init; }
    public int NewDay { get; init; }
    public bool IsBossDay { get; init; }
    public string Message { get; init; } = "";
}

public sealed class UseCaseResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static UseCaseResult Ok() => new() { Success = true };
    public static UseCaseResult Fail(string error) => new() { Success = false, Error = error };
}

public sealed class EquipResultDto
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string Message { get; init; } = "";
    public string? SwapMessage { get; init; }

    public static EquipResultDto Ok(string message, string? swapMessage) =>
        new() { Success = true, Message = message, SwapMessage = swapMessage };

    public static EquipResultDto Fail(string error) =>
        new() { Success = false, Error = error };
}

public sealed class UseItemResultDto
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string Message { get; init; } = "";

    public static UseItemResultDto Fail(string error) =>
        new() { Success = false, Error = error };
}

public sealed class InventoryItemDto
{
    public int Index { get; init; }
    public string ItemId { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public int Quantity { get; init; }
    public bool CanEquip { get; init; }
    public bool CanUse { get; init; }
}

public sealed class EquipmentDto
{
    public string? WeaponName { get; init; }
    public string? ArmorName { get; init; }
    public string? RingName { get; init; }
    public int EffectiveAttack { get; init; }
    public int EffectiveDefense { get; init; }
}

public sealed class MerchantResultDto
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string Message { get; init; } = "";

    public static MerchantResultDto Ok(string message) =>
        new() { Success = true, Message = message };

    public static MerchantResultDto Fail(string error) =>
        new() { Success = false, Error = error };
}

public sealed class SaveResultDto
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string Message { get; init; } = "";

    public static SaveResultDto Ok(string message) =>
        new() { Success = true, Message = message };

    public static SaveResultDto Fail(string error) =>
        new() { Success = false, Error = error };
}

public sealed class LoadResultDto
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string Message { get; init; } = "";
    public GameSession? Session { get; init; }

    public static LoadResultDto Ok(GameSession session, string message) =>
        new() { Success = true, Session = session, Message = message };

    public static LoadResultDto Fail(string error) =>
        new() { Success = false, Error = error };
}
