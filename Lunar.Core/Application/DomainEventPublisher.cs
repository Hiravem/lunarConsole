using Lunar.Core.Application.Interfaces;
using Lunar.Core.Domain.Events;

namespace Lunar.Core.Application;

public static class DomainEventPublisher
{
    public static void PublishAll(IEventBus eventBus, IEnumerable<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
            eventBus.Publish(domainEvent);
    }
}
