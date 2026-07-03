using Lunar.Core.Util;
using Lunar.Core.Model.Characters;

namespace Lunar.Core.Model.Combat;

public static class CriticalCalculator
{
    public const double DefaultMultiplier = 1.5;

    public static bool RollCrit(Stats attacker, IRandomService random) =>
        attacker.CritChance > 0 && random.Next(100) < attacker.CritChance;

    public static int ApplyMultiplier(int baseDamage, double multiplier = DefaultMultiplier) =>
        Math.Max(1, (int)(baseDamage * multiplier));
}
