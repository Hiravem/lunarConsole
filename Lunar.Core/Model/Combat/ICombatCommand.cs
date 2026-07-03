namespace Lunar.Core.Model.Combat;

public interface ICombatCommand
{
    CommandResult Execute(CombatSession session);
}
