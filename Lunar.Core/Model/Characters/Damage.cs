namespace Lunar.Core.Model.Characters;

public enum DamageType
{
    Physical,
    Magical
}

public sealed record Damage(int Amount, DamageType Type = DamageType.Physical);
