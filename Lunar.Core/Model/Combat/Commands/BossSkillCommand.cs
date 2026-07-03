using Lunar.Core.Util;
using Lunar.Core.Model.Characters;
using Lunar.Core.Model.Combat;

namespace Lunar.Core.Model.Combat.Commands;

public sealed class BossSkillCommand : ICombatCommand
{
    private readonly string _skillName;
    private readonly double _multiplier;
    private readonly IRandomService? _random;

    public BossSkillCommand(string skillName, double multiplier, IRandomService? random = null)
    {
        _skillName = skillName;
        _multiplier = multiplier;
        _random = random;
    }

    public CommandResult Execute(CombatSession session)
    {
        var player = session.Player;
        var enemy = session.Enemy;
        var damage = DamageCalculator.CalculateWithMultiplier(
            enemy.Stats, player.GetEffectiveStats(), _multiplier, _random);
        var applied = player.TakeDamage(damage);

        var lines = new List<string>
        {
            $"{enemy.Name} uses {_skillName}!",
            $"Player takes {applied} damage!",
            $"{player.Name} HP: {player.Health.Current}/{player.Health.Max}"
        };

        if (!player.IsAlive)
        {
            session.EndCombat(CombatWinner.Enemy);
            session.AddEvent(new Events.PlayerDied());
            lines.Add($"{player.Name} has fallen...");
        }

        return CommandResult.Ok(lines.ToArray());
    }
}
