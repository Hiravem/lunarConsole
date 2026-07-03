namespace Lunar.Core.Model.Combat.Commands;

public sealed class FleeCommand : ICombatCommand
{
    public CommandResult Execute(CombatSession session)
    {
        session.Flee();
        return CommandResult.Ok("You fled from combat safely.", "No loot obtained.");
    }
}
