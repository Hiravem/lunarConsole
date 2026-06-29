using Lunar.Core.Domain.Characters;

namespace Lunar.Core.Domain.Combat;

public interface IEnemyAI
{
    ICombatCommand ChooseAction(CombatSession session);
}

public sealed class SimpleEnemyAI : IEnemyAI
{
    public static readonly SimpleEnemyAI Instance = new();

    public ICombatCommand ChooseAction(CombatSession session) => new Commands.AttackCommand(isPlayer: false);
}
