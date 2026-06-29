using Lunar.Core.Domain.Events;

namespace Lunar.Core.Application.Interfaces;

public interface IEventBus
{
    void Publish(IDomainEvent domainEvent);
    void Subscribe<T>(Action<T> handler) where T : IDomainEvent;
}
