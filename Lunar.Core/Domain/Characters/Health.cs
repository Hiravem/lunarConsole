namespace Lunar.Core.Domain.Characters;

public sealed class Health
{
    public int Current { get; private set; }
    public int Max { get; }

    public Health(int max, int? current = null)
    {
        Max = max;
        Current = current ?? max;
    }

    public bool IsDead => Current <= 0;

    public int TakeDamage(int amount)
    {
        var applied = Math.Min(Current, Math.Max(0, amount));
        Current -= applied;
        return applied;
    }

    public int Heal(int amount)
    {
        var before = Current;
        Current = Math.Min(Max, Current + amount);
        return Current - before;
    }

    public Health Clone() => new(Max, Current);
}
