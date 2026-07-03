using Lunar.Core.Model.Dto;

namespace Lunar.Console.UI.Screens;

public enum GameplayAction
{
    Explore = 1,
    Rest = 2,
    Inventory = 3,
    Equipment = 4,
    FaceBoss = 5,
    NextDay = 6,
    Save = 7,
    ExitToMenu = 8
}

public sealed class GameplayScreen
{
    private readonly InputReader _input;
    private readonly OutputWriter _output;

    public GameplayScreen(InputReader input, OutputWriter output)
    {
        _input = input;
        _output = output;
    }

    public GameplayAction Show(GameplayMenuDto dto)
    {
        _output.WriteLine();
        _output.WriteHeader("LUNAR — Gameplay");
        _output.WriteLine($"Day : {dto.Day}");
        _output.WriteLine();
        _output.WriteLine($"HP  : {dto.HpCurrent} / {dto.HpMax}");
        _output.WriteLine($"ATK : {dto.Attack}  (effective)");
        _output.WriteLine($"DEF : {dto.Defense}");
        _output.WriteLine($"Gold: {dto.Gold}");
        _output.WriteLine($"Difficulty: {dto.Difficulty}  |  Bosses defeated: {dto.BossesDefeated}");
        _output.WriteLine();

        if (dto.IsBossDay)
            _output.WriteLine("*** BOSS DAY — Face the boss! ***");
        else
            _output.WriteLine($"Boss in: {dto.DaysUntilBoss} day(s)");

        var dayStatus = dto.HasExplored ? "Explored" : dto.HasRested ? "Rested" : "No main action yet";
        _output.WriteLine($"Today: [{dayStatus}]");
        _output.WriteLine($"Inventory: {dto.InventorySummary}");
        _output.WriteLine($"Equipment: {dto.EquipmentSummary}");
        _output.WriteSeparator('-');

        if (dto.IsBossDay)
        {
            _output.WriteLine("1. Face Boss");
            _output.WriteLine("2. Inventory");
            _output.WriteLine("3. Equipment");
            _output.WriteLine("4. Save");
            _output.WriteLine("5. Exit to Menu");

            return _input.ReadChoice("> ", 1, 5) switch
            {
                1 => GameplayAction.FaceBoss,
                2 => GameplayAction.Inventory,
                3 => GameplayAction.Equipment,
                4 => GameplayAction.Save,
                _ => GameplayAction.ExitToMenu
            };
        }

        _output.WriteLine("1. Explore          (1/day)");
        _output.WriteLine("2. Rest             (1/day)");
        _output.WriteLine("3. Inventory");
        _output.WriteLine("4. Equipment");
        _output.WriteLine("5. Next Day");
        _output.WriteLine("6. Save");
        _output.WriteLine("7. Exit to Menu");

        return _input.ReadChoice("> ", 1, 7) switch
        {
            1 => GameplayAction.Explore,
            2 => GameplayAction.Rest,
            3 => GameplayAction.Inventory,
            4 => GameplayAction.Equipment,
            5 => GameplayAction.NextDay,
            6 => GameplayAction.Save,
            _ => GameplayAction.ExitToMenu
        };
    }
}
