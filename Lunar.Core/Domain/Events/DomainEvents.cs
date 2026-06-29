namespace Lunar.Core.Domain.Events;

public interface IDomainEvent;

public sealed record EnemyDefeated(string EnemyId, string LootTableId) : IDomainEvent;

public sealed record LootGranted(IReadOnlyList<string> ItemIds) : IDomainEvent;

public sealed record DayAdvanced(int DayNumber) : IDomainEvent;

public sealed record ItemEquipped(string ItemId, string Slot) : IDomainEvent;

public sealed record PlayerDied() : IDomainEvent;

public sealed record BossPhaseChanged(string BossId, string PhaseId, string PhaseName) : IDomainEvent;

public sealed record ChestOpened(string LootTableId) : IDomainEvent;

public sealed record BossDefeated(string BossId, string LootTableId) : IDomainEvent;

public sealed record GameSaved(int DayNumber, int Difficulty) : IDomainEvent;
