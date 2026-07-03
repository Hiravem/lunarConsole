namespace Lunar.Core.Exception;

public sealed class EntityNotFoundException : GameException
{
    public string EntityType { get; }
    public string EntityId { get; }

    public EntityNotFoundException(string entityType, string entityId)
        : base($"Unknown {entityType}: {entityId}")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
