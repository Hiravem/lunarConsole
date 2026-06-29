using Lunar.Core.Application.Interfaces;
using Lunar.Core.Domain.Characters;

namespace Lunar.Core.Domain.Combat;

public static class DamageCalculator
{
    public static Damage Calculate(Stats attacker, Stats defender, IRandomService? random = null)
    {
        var baseDamage = Math.Max(1, attacker.Attack - defender.Defense / 2);

        if (random is not null && CriticalCalculator.RollCrit(attacker, random))
        {
            var critDamage = CriticalCalculator.ApplyMultiplier(baseDamage);
            return new Damage(critDamage);
        }

        return new Damage(baseDamage);
    }

    public static Damage CalculateWithMultiplier(
        Stats attacker,
        Stats defender,
        double multiplier,
        IRandomService? random = null)
    {
        var baseDamage = Math.Max(1, (int)((attacker.Attack - defender.Defense / 2) * multiplier));

        if (random is not null && CriticalCalculator.RollCrit(attacker, random))
            baseDamage = CriticalCalculator.ApplyMultiplier(baseDamage);

        return new Damage(Math.Max(1, baseDamage));
    }
}
