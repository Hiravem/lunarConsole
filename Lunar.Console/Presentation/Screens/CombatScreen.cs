using Lunar.Core.Application;
using Lunar.Core.Application.UseCases;
using Lunar.Core.Domain.Bosses;
using Lunar.Core.Domain.Combat;
using Lunar.Core.Domain.Skills;

namespace Lunar.Console.Presentation.Screens;

public enum CombatAction
{
    Attack = 1,
    Skill = 2,
    Item = 3,
    Flee = 4,
    Back = 0
}

public sealed class CombatScreen
{
    private readonly InputReader _input;
    private readonly OutputWriter _output;

    public CombatScreen(InputReader input, OutputWriter output)
    {
        _input = input;
        _output = output;
    }

    public CombatActionResult Show(GameSession session, CombatSession combat)
    {
        var stats = session.Player.GetEffectiveStats();
        var skillCd = session.Player.SkillState.GetCooldown(SkillDefinition.HeroStrike.Id);
        var potions = session.Player.Inventory.GetQuantity("health_potion");
        var isBoss = combat.Enemy.IsBoss;

        _output.WriteLine();
        _output.WriteHeader(isBoss ? "Boss Battle" : "Combat");
        _output.WriteLine("Player");
        _output.WriteLine($"HP  : {session.Player.Health.Current}/{session.Player.Health.Max}   ATK: {stats.Attack}");
        if (skillCd > 0)
            _output.WriteLine($"Skill cooldown: {skillCd} turn(s)");
        _output.WriteSeparator('-');

        if (combat.Enemy is Boss boss)
            _output.WriteLine($"{boss.Name}  [Phase {boss.PhaseNumber}: {boss.CurrentPhase.PhaseName}]");
        else
            _output.WriteLine(combat.Enemy.Name);

        _output.WriteLine($"HP  : {combat.Enemy.Health.Current}/{combat.Enemy.Health.Max}");
        _output.WriteSeparator('-');
        _output.WriteLine("1. Attack");
        _output.WriteLine($"2. Hero Strike{(skillCd > 0 ? " (cooldown)" : "")}");
        _output.WriteLine($"3. Use Potion (x{potions})");
        _output.WriteLine("4. Run");
        _output.WriteLine();

        var choice = _input.ReadChoice("> ", 1, 4);

        return choice switch
        {
            1 => CombatActionResult.Attack(),
            2 => CombatActionResult.Skill(),
            3 => CombatActionResult.Item("health_potion"),
            _ => CombatActionResult.Flee()
        };
    }
}

public sealed class CombatActionResult
{
    public CombatAction Action { get; init; }
    public string? ItemId { get; init; }

    public static CombatActionResult Attack() => new() { Action = CombatAction.Attack };
    public static CombatActionResult Skill() => new() { Action = CombatAction.Skill };
    public static CombatActionResult Item(string itemId) => new() { Action = CombatAction.Item, ItemId = itemId };
    public static CombatActionResult Flee() => new() { Action = CombatAction.Flee };
}
