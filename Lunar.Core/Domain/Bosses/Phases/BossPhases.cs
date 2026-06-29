using Lunar.Core.Domain.Combat;
using Lunar.Core.Domain.Combat.Commands;

namespace Lunar.Core.Domain.Bosses.Phases;

public sealed class Phase1Behavior : IBossPhase
{
    public static readonly Phase1Behavior Instance = new();

    public string PhaseId => "phase_1";
    public string PhaseName => "Phase 1";

    public ICombatCommand ChooseAction(CombatSession session) =>
        new AttackCommand(isPlayer: false);
}

public sealed class Phase2Behavior : IBossPhase
{
    public static readonly Phase2Behavior Instance = new();

    public string PhaseId => "phase_2";
    public string PhaseName => "Phase 2";

    public ICombatCommand ChooseAction(CombatSession session)
    {
        var roll = Random.Shared.Next(100);
        return roll < 50
            ? new BossSkillCommand("Fireball", multiplier: 1.8)
            : new AttackCommand(isPlayer: false);
    }
}

public sealed class EnragedBehavior : IBossPhase
{
    public static readonly EnragedBehavior Instance = new();

    public string PhaseId => "phase_enraged";
    public string PhaseName => "Enraged";

    public ICombatCommand ChooseAction(CombatSession session) =>
        new BossSkillCommand("Soul Burst", multiplier: 2.5);
}
