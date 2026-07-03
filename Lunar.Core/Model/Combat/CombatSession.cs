using Lunar.Core.Model.Characters;

namespace Lunar.Core.Model.Combat;

public enum CombatPhase
{
    PlayerTurn,
    EnemyTurn,
    Finished
}

public enum CombatWinner
{
    Player,
    Enemy
}

public sealed class CombatSession
{
    private readonly List<Events.IDomainEvent> _pendingEvents = new();

    public Player Player { get; }
    public Enemy Enemy { get; }
    public CombatPhase Phase { get; private set; } = CombatPhase.PlayerTurn;
    public bool IsFinished { get; private set; }
    public bool Fled { get; private set; }
    public CombatWinner? Winner { get; private set; }
    public IReadOnlyList<Events.IDomainEvent> PendingEvents => _pendingEvents;

    public CombatSession(Player player, Enemy enemy)
    {
        Player = player;
        Enemy = enemy;
    }

    public CommandResult Execute(ICombatCommand command)
    {
        if (IsFinished)
            return CommandResult.Fail("Combat has already ended.");

        if (Phase != CombatPhase.PlayerTurn)
            return CommandResult.Fail("It is not the player's turn.");

        var result = command.Execute(this);
        if (!result.Success || IsFinished)
            return result;

        if (command is Commands.FleeCommand)
            return result;

        var logLines = result.LogLines.ToList();
        ProcessEnemyTurn(logLines);
        Player.SkillState.TickCooldowns();
        return CommandResult.Ok(logLines.ToArray());
    }

    public void ProcessEnemyTurn(List<string>? logLines = null)
    {
        if (IsFinished) return;

        Phase = CombatPhase.EnemyTurn;
        var enemyCommand = Enemy.AI.ChooseAction(this);
        var enemyResult = enemyCommand.Execute(this);
        logLines?.AddRange(enemyResult.LogLines);
        Phase = IsFinished ? CombatPhase.Finished : CombatPhase.PlayerTurn;
    }

    public void EndCombat(CombatWinner winner)
    {
        IsFinished = true;
        Winner = winner;
        Phase = CombatPhase.Finished;
    }

    public void Flee()
    {
        Fled = true;
        IsFinished = true;
        Phase = CombatPhase.Finished;
    }

    public void AddEvent(Events.IDomainEvent domainEvent) => _pendingEvents.Add(domainEvent);

    public void OnEnemyDamaged()
    {
        if (Enemy is Bosses.Boss boss)
            boss.CheckPhaseTransition(this);
    }

    public IReadOnlyList<Events.IDomainEvent> DrainEvents()
    {
        var copy = _pendingEvents.ToList();
        _pendingEvents.Clear();
        return copy;
    }
}
