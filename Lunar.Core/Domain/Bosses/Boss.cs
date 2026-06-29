using Lunar.Core.Domain.Bosses.Phases;
using Lunar.Core.Domain.Combat;

namespace Lunar.Core.Domain.Bosses;

public sealed class BossPhaseAI : IEnemyAI
{
    private readonly Boss _boss;

    public BossPhaseAI(Boss boss) => _boss = boss;

    public ICombatCommand ChooseAction(CombatSession session) =>
        _boss.CurrentPhase.ChooseAction(session);
}

public sealed class Boss : Characters.Enemy
{
    private readonly IBossPhase[] _phases;

    public IBossPhase CurrentPhase { get; private set; }
    public int PhaseNumber { get; private set; } = 1;

    public Boss(
        string id,
        string name,
        Characters.Health health,
        Characters.Stats stats,
        string lootTableId,
        params IBossPhase[] phases)
        : base(id, name, health, stats, lootTableId, SimpleEnemyAI.Instance)
    {
        _phases = phases.Length > 0
            ? phases
            : new IBossPhase[] { Phase1Behavior.Instance, Phase2Behavior.Instance, EnragedBehavior.Instance };

        CurrentPhase = _phases[0];
        AI = new BossPhaseAI(this);
    }

    public void CheckPhaseTransition(CombatSession session)
    {
        if (!IsAlive) return;

        var hpPercent = (double)Health.Current / Health.Max * 100;
        var targetIndex = hpPercent switch
        {
            > 66 => 0,
            > 33 => 1,
            _ => 2
        };

        if (targetIndex >= _phases.Length)
            targetIndex = _phases.Length - 1;

        var nextPhase = _phases[targetIndex];
        if (nextPhase.PhaseId == CurrentPhase.PhaseId)
            return;

        CurrentPhase = nextPhase;
        PhaseNumber = targetIndex + 1;
        session.AddEvent(new Events.BossPhaseChanged(Id, CurrentPhase.PhaseId, CurrentPhase.PhaseName));
    }
}
