namespace Lunar.Core.Domain.Combat;

public interface ICombatCommand
{
    CommandResult Execute(CombatSession session);
}
