using Lunar.Core.Util;
using Lunar.Core.Model.Characters;
using Lunar.Core.Model.Events;
using Lunar.Core.Model.Skills;

namespace Lunar.Core.Model.Combat.Commands;

public sealed class SkillCommand : ICombatCommand
{
    private readonly SkillDefinition _skill;
    private readonly IRandomService? _random;

    public SkillCommand(SkillDefinition skill, IRandomService? random = null)
    {
        _skill = skill;
        _random = random;
    }

    public CommandResult Execute(CombatSession session)
    {
        var player = session.Player;
        var enemy = session.Enemy;

        if (!player.SkillState.CanUse(_skill.Id))
        {
            var cd = player.SkillState.GetCooldown(_skill.Id);
            return CommandResult.Fail($"{_skill.Name} on cooldown ({cd} turn(s) left).");
        }

        var damage = DamageCalculator.CalculateWithMultiplier(
            player.GetEffectiveStats(), enemy.Stats, _skill.Multiplier, _random);
        var applied = enemy.TakeDamage(damage);

        player.SkillState.MarkUsed(_skill.Id, _skill.CooldownTurns);

        var lines = new List<string>
        {
            $"{player.Name} uses {_skill.Name}!",
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
}
