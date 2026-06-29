namespace Lunar.Core.Domain.Characters;

public interface IDamageable
{
    Health Health { get; }
    Stats Stats { get; }
    bool IsAlive { get; }
    int TakeDamage(Damage damage);
}
