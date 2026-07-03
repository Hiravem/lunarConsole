using Lunar.Core.Model.Events;

namespace Lunar.Core.Util;

public static class DomainEventPublisher
{
    public static void PublishAll(IEventBus eventBus, IEnumerable<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
            eventBus.Publish(domainEvent);
    }
}
