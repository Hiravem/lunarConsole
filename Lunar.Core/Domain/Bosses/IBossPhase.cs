using Lunar.Core.Domain.Combat;

namespace Lunar.Core.Domain.Bosses;

public interface IBossPhase
{
    string PhaseId { get; }
    string PhaseName { get; }
    ICombatCommand ChooseAction(CombatSession session);
}
