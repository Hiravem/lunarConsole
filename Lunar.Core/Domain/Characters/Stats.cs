namespace Lunar.Core.Domain.Characters;

public sealed class Stats
{
    public int Attack { get; }
    public int Defense { get; }
    public int CritChance { get; }

    public Stats(int attack, int defense, int critChance = 0)
    {
        Attack = attack;
        Defense = defense;
        CritChance = critChance;
    }

    public Stats ApplyModifier(Stats modifier) =>
        new(Attack + modifier.Attack, Defense + modifier.Defense, CritChance + modifier.CritChance);

    public static Stats Zero => new(0, 0, 0);
}
