using Lunar.Core.Model.Combat;

namespace Lunar.Core.Model.Bosses;

public interface IBossPhase
{
    string PhaseId { get; }
    string PhaseName { get; }
    ICombatCommand ChooseAction(CombatSession session);
}
