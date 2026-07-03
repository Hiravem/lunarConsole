using Lunar.Core.Model;
using Lunar.Core.Model.Characters;
using Lunar.Core.Model.Events;
using Lunar.Core.Model.Items;
using Lunar.Core.Util;

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

    public void Publish(IDomainEvent domainEvent) =>
        Published.Add(domainEvent);

    public void Subscribe<T>(Action<T> handler) where T : IDomainEvent { }
}

public static class TestHelpers
{
    public static GameSession CreateSession(ItemFactory factory)
    {
        var player = new Player(
            "Hero",
            new Health(120),
            new Stats(15, 5, critChance: 10),
            factory,
            gold: 0);

        return GameSession.CreateNew(player);
    }
}
