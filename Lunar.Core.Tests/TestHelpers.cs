using Lunar.Core.Application.Interfaces;

namespace Lunar.Core.Tests;

public sealed class FixedRandomService : IRandomService
{
    private readonly Queue<int> _values;

    public FixedRandomService(params int[] values) =>
        _values = new Queue<int>(values);

    public int Next(int maxExclusive)
    {
        if (_values.Count == 0) return 0;
        var v = _values.Dequeue();
        return Math.Abs(v) % Math.Max(1, maxExclusive);
    }

    public int Next(int minInclusive, int maxExclusive) =>
        minInclusive + Next(maxExclusive - minInclusive);
}

public sealed class InMemoryEventBus : IEventBus
{
    public List<object> Published { get; } = new();

    public void Publish(Domain.Events.IDomainEvent domainEvent) =>
        Published.Add(domainEvent);

    public void Subscribe<T>(Action<T> handler) where T : Domain.Events.IDomainEvent { }
}

public static class TestHelpers
{
    public static Application.GameSession CreateSession(Domain.Items.ItemFactory factory)
    {
        var player = new Domain.Characters.Player(
            "Hero",
            new Domain.Characters.Health(120),
            new Domain.Characters.Stats(15, 5, critChance: 10),
            factory,
            gold: 0);

        return Application.GameSession.CreateNew(player);
    }
}
