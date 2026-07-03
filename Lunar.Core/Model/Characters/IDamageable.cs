namespace Lunar.Core.Model.Characters;

public interface IDamageable
{
    Health Health { get; }
    Stats Stats { get; }
    bool IsAlive { get; }
    int TakeDamage(Damage damage);
}
