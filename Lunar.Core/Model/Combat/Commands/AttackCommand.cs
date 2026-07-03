using Lunar.Core.Util;
using Lunar.Core.Model.Characters;
using Lunar.Core.Model.Events;

namespace Lunar.Core.Model.Combat.Commands;

public sealed class AttackCommand : ICombatCommand
{
    private readonly bool _isPlayer;
    private readonly IRandomService? _random;

    public AttackCommand(bool isPlayer, IRandomService? random = null)
    {
        _isPlayer = isPlayer;
        _random = random;
    }

    public CommandResult Execute(CombatSession session)
    {
        if (_isPlayer)
            return ExecutePlayerAttack(session);

        return ExecuteEnemyAttack(session);
    }

    private CommandResult ExecutePlayerAttack(CombatSession session)
    {
        var player = session.Player;
        var enemy = session.Enemy;
        var damage = DamageCalculator.Calculate(player.GetEffectiveStats(), enemy.Stats, _random);
        var applied = enemy.TakeDamage(damage);

        var lines = new List<string>
        {
            $"{player.Name} attacks!",
            $"Damage: {applied}",
            $"{enemy.Name} HP: {enemy.Health.Current}/{enemy.Health.Max}"
        };

        if (!enemy.IsAlive)
        {
            session.EndCombat(CombatWinner.Player);
            session.AddEvent(new EnemyDefeated(enemy.Id, enemy.LootTableId));
            lines.Add($"{enemy.Name} defeated!");
        }
        else
        {
            session.OnEnemyDamaged();
        }

        return CommandResult.Ok(lines.ToArray());
    }

    private CommandResult ExecuteEnemyAttack(CombatSession session)
    {
        var player = session.Player;
        var enemy = session.Enemy;
        var damage = DamageCalculator.Calculate(enemy.Stats, player.GetEffectiveStats(), _random);
        var applied = player.TakeDamage(damage);

        var lines = new List<string>
        {
            $"{enemy.Name} attacks!",
            $"Damage: {applied}",
            $"{player.Name} HP: {player.Health.Current}/{player.Health.Max}"
        };

        if (!player.IsAlive)
        {
            session.EndCombat(CombatWinner.Enemy);
            session.AddEvent(new PlayerDied());
            lines.Add($"{player.Name} has fallen...");
        }

        return CommandResult.Ok(lines.ToArray());
    }
}
