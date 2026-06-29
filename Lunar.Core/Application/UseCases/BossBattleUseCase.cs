using Lunar.Core.Application.DTOs;

namespace Lunar.Core.Application.UseCases;

public sealed class BossBattleUseCase
{
    private readonly CombatUseCase _combatUseCase;

    public BossBattleUseCase(CombatUseCase combatUseCase) =>
        _combatUseCase = combatUseCase;

    public UseCaseResult StartBossFight(GameSession session, string bossId = "skeleton_king")
    {
        if (!session.IsBossDay)
            return UseCaseResult.Fail("No boss on this day.");

        if (session.ActiveCombatSession is not null)
            return UseCaseResult.Fail("Already in combat.");

        session.DayFlags.HasExplored = true;
        return _combatUseCase.StartBossCombat(session, bossId);
    }

    public CombatResultDto Attack(GameSession session) => _combatUseCase.Attack(session);
    public CombatResultDto UseItem(GameSession session, string itemId) => _combatUseCase.UseItem(session, itemId);
    public CombatResultDto Skill(GameSession session) => _combatUseCase.Skill(session);
    public CombatResultDto Flee(GameSession session) => _combatUseCase.Flee(session);
}
