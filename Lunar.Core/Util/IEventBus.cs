using Lunar.Core.Model.Events;

namespace Lunar.Core.Util;

public interface IEventBus
{
    void Publish(IDomainEvent domainEvent);
    void Subscribe<T>(Action<T> handler) where T : IDomainEvent;
}
