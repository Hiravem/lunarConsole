using Lunar.Core.Application.DTOs;
using Lunar.Core.Application.Interfaces;
using Lunar.Core.Domain.Bosses;
using Lunar.Core.Domain.Characters;
using Lunar.Core.Domain.Combat;
using Lunar.Core.Domain.Combat.Commands;
using Lunar.Core.Domain.Events;
using Lunar.Core.Domain.Skills;
using Lunar.Core.Domain.World;

namespace Lunar.Core.Application.UseCases;

public sealed class CombatUseCase
{
    private readonly EnemyFactory _enemyFactory;
    private readonly BossFactory _bossFactory;
    private readonly ApplyLootUseCase _applyLoot;
    private readonly IEventBus _eventBus;
    private readonly IRandomService _random;

    public CombatUseCase(
        EnemyFactory enemyFactory,
        BossFactory bossFactory,
        ApplyLootUseCase applyLoot,
        IEventBus eventBus,
        IRandomService random)
    {
        _enemyFactory = enemyFactory;
        _bossFactory = bossFactory;
        _applyLoot = applyLoot;
        _eventBus = eventBus;
        _random = random;
    }

    public UseCaseResult StartCombat(GameSession session, string enemyId)
    {
        if (session.ActiveCombatSession is not null)
            return UseCaseResult.Fail("Combat already in progress.");

        var enemy = _enemyFactory.Create(enemyId, session.Difficulty);
        session.ActiveCombatSession = new CombatSession(session.Player, enemy);
        return UseCaseResult.Ok();
    }

    public UseCaseResult StartBossCombat(GameSession session, string bossId)
    {
        if (session.ActiveCombatSession is not null)
            return UseCaseResult.Fail("Combat already in progress.");

        var boss = _bossFactory.Create(bossId, session.Difficulty);
        session.ActiveCombatSession = new CombatSession(session.Player, boss);
        return UseCaseResult.Ok();
    }

    public CombatResultDto ExecuteCommand(GameSession session, ICombatCommand command)
    {
        var combat = session.ActiveCombatSession;
        if (combat is null)
            return new CombatResultDto { Success = false, Error = "No active combat." };

        var result = combat.Execute(command);
        if (!result.Success)
            return new CombatResultDto { Success = false, Error = result.Error };

        var events = combat.DrainEvents();
        var logLines = result.LogLines.ToList();
        AppendPhaseMessages(logLines, events, combat.Enemy);

        DomainEventPublisher.PublishAll(_eventBus, events);

        string? lootMessage = null;
        var playerDied = false;
        var playerWon = false;

        if (combat.IsFinished)
        {
            if (combat.Fled)
            {
                session.ActiveCombatSession = null;
            }
            else if (combat.Winner == CombatWinner.Player)
            {
                playerWon = true;
                lootMessage = combat.Enemy.IsBoss
                    ? _applyLoot.ApplyBossRewards(session, combat.Enemy)
                    : _applyLoot.ApplyFromLootTable(session, combat.Enemy.LootTableId);
                session.ActiveCombatSession = null;
            }
            else if (combat.Winner == CombatWinner.Enemy)
            {
                playerDied = true;
                session.ActiveCombatSession = null;
            }
        }

        return new CombatResultDto
        {
            Success = true,
            LogLines = logLines,
            CombatEnded = combat.IsFinished,
            PlayerWon = playerWon,
            Fled = combat.Fled,
            PlayerDied = playerDied,
            LootMessage = lootMessage,
            BossPhase = GetBossPhase(combat.Enemy)
        };
    }

    public CombatResultDto Attack(GameSession session) =>
        ExecuteCommand(session, new AttackCommand(isPlayer: true, _random));

    public CombatResultDto UseItem(GameSession session, string itemId) =>
        ExecuteCommand(session, new UseItemCommand(itemId, session.Player.ItemFactory));

    public CombatResultDto Skill(GameSession session) =>
        ExecuteCommand(session, new SkillCommand(SkillDefinition.HeroStrike, _random));

    public CombatResultDto Flee(GameSession session) =>
        ExecuteCommand(session, new FleeCommand());

    private static void AppendPhaseMessages(
        List<string> logLines,
        IReadOnlyList<IDomainEvent> events,
        Enemy enemy)
    {
        foreach (var evt in events)
        {
            if (evt is BossPhaseChanged phase)
                logLines.Add($"{enemy.Name} enters {phase.PhaseName}!");
        }
    }

    private static int? GetBossPhase(Enemy enemy) =>
        enemy is Boss boss ? boss.PhaseNumber : null;
}
