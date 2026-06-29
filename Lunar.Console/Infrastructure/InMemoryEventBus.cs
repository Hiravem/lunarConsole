using Lunar.Core.Application.Interfaces;
using Lunar.Core.Domain.Events;

namespace Lunar.Console.Infrastructure;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Publish(IDomainEvent domainEvent)
    {
        var type = domainEvent.GetType();
        if (!_handlers.TryGetValue(type, out var handlers)) return;

        foreach (var handler in handlers.ToArray())
            handler.DynamicInvoke(domainEvent);
    }

    public void Subscribe<T>(Action<T> handler) where T : IDomainEvent
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list))
        {
            list = new List<Delegate>();
            _handlers[type] = list;
        }

        list.Add(handler);
    }
}
