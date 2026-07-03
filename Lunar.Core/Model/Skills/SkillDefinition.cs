namespace Lunar.Core.Model.Skills;

public sealed class SkillDefinition
{
    public string Id { get; }
    public string Name { get; }
    public double Multiplier { get; }
    public int CooldownTurns { get; }

    public SkillDefinition(string id, string name, double multiplier, int cooldownTurns)
    {
        Id = id;
        Name = name;
        Multiplier = multiplier;
        CooldownTurns = cooldownTurns;
    }

    public static SkillDefinition HeroStrike { get; } = new(
        "hero_strike", "Hero Strike", multiplier: 1.5, cooldownTurns: 2);
}

public sealed class PlayerSkillState
{
    private readonly Dictionary<string, int> _cooldowns = new();

    public bool CanUse(string skillId) =>
        !_cooldowns.TryGetValue(skillId, out var turns) || turns <= 0;

    public int GetCooldown(string skillId) =>
        _cooldowns.TryGetValue(skillId, out var turns) ? turns : 0;

    public void MarkUsed(string skillId, int cooldownTurns) =>
        _cooldowns[skillId] = cooldownTurns;

    public void TickCooldowns()
    {
        var keys = _cooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            _cooldowns[key]--;
            if (_cooldowns[key] <= 0)
                _cooldowns.Remove(key);
        }
    }
}
