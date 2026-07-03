using Lunar.Core.Model;
using Lunar.Core.Model.Dto;
using Lunar.Core.Util;
using Lunar.Core.Model.World;

namespace Lunar.Core.Service;

public sealed class ExploreUseCase
{
    private readonly IRandomService _random;
    private readonly ApplyLootUseCase _applyLoot;
    private readonly EncounterTable _encounterTable;

    public ExploreUseCase(
        IRandomService random,
        ApplyLootUseCase applyLoot,
        EncounterTable? encounterTable = null)
    {
        _random = random;
        _applyLoot = applyLoot;
        _encounterTable = encounterTable ?? EncounterTable.Default;
    }

    public ExploreResultDto Execute(GameSession session)
    {
        if (session.IsBossDay)
            return Fail("Boss day! Face the boss instead of exploring.");

        if (session.DayFlags.HasUsedDayAction)
            return Fail("You already used your main action today (explore or rest).");

        if (session.ActiveCombatSession is not null)
            return Fail("Already in combat.");

        session.DayFlags.HasExplored = true;

        var encounter = _encounterTable.PickRandom(_random);
        var context = new ExploreContext { Difficulty = session.Difficulty, Random = _random };
        var result = ExploreResolver.Resolve(encounter, context);

        return MapResult(session, result);
    }

    private ExploreResultDto MapResult(GameSession session, EncounterResult result)
    {
        string? effectMessage = result.Type switch
        {
            EncounterResultType.Loot when result.LootTableId is not null =>
                _applyLoot.ApplyFromChest(session, result.LootTableId),
            EncounterResultType.Event =>
                ApplyEventEffect(session, result),
            _ => null
        };

        return new ExploreResultDto
        {
            Success = true,
            Message = result.Message,
            EncounterType = MapEncounterType(result.Type),
            StartsCombat = result.StartsCombat,
            EnemyId = result.EnemyId,
            OpensMerchant = result.OpensMerchant,
            EffectMessage = effectMessage
        };
    }

    private static string ApplyEventEffect(GameSession session, EncounterResult result)
    {
        if (result.GoldChange is int gold and > 0)
        {
            session.Player.AddGold(gold);
            return $"{result.Message} (+{gold} gold)";
        }

        if (result.HealAmount is int heal and > 0)
        {
            var healed = session.Player.Health.Heal(heal);
            return $"{result.Message} (Healed {healed} HP)";
        }

        if (result.DamageAmount is int damage and > 0)
        {
            var taken = session.Player.Health.TakeDamage(damage);
            return $"{result.Message} (Took {taken} damage)";
        }

        return result.Message;
    }

    private static ExploreEncounterType MapEncounterType(EncounterResultType type) => type switch
    {
        EncounterResultType.Combat => ExploreEncounterType.Combat,
        EncounterResultType.Loot => ExploreEncounterType.Loot,
        EncounterResultType.Merchant => ExploreEncounterType.Merchant,
        EncounterResultType.Event => ExploreEncounterType.Event,
        _ => ExploreEncounterType.Narrative
    };

    private static ExploreResultDto Fail(string error) =>
        new() { Success = false, Error = error };
}
