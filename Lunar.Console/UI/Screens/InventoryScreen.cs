using Lunar.Core.Model;
using Lunar.Core.Model.Dto;
using Lunar.Core.Service;

namespace Lunar.Console.UI.Screens;

public sealed class InventoryScreen
{
    private readonly InputReader _input;
    private readonly OutputWriter _output;
    private readonly UseItemUseCase _useItem;
    private readonly EquipItemUseCase _equipItem;

    public InventoryScreen(
        InputReader input,
        OutputWriter output,
        UseItemUseCase useItem,
        EquipItemUseCase equipItem)
    {
        _input = input;
        _output = output;
        _useItem = useItem;
        _equipItem = equipItem;
    }

    public void Show(GameSession session)
    {
        while (true)
        {
            var items = GameplayPresenterMapper.ToInventoryDto(session.Player);

            _output.WriteLine();
            _output.WriteHeader("Inventory");

            if (items.Count == 0)
            {
                _output.WriteLine("(empty)");
                _output.Pause();
                return;
            }

            foreach (var item in items)
            {
                var tags = new List<string>();
                if (item.CanEquip) tags.Add("equip");
                if (item.CanUse) tags.Add("use");
                var tagStr = tags.Count > 0 ? $" [{string.Join(", ", tags)}]" : "";
                _output.WriteLine($"{item.Index}. {item.DisplayName} x{item.Quantity}{tagStr}");
            }

            _output.WriteLine();
            _output.WriteLine("Enter item # to manage, 0 to back");
            var choice = _input.ReadChoice("> ", 0, items.Count);

            if (choice == 0) return;

            var selected = items[choice - 1];
            HandleItem(session, selected);
        }
    }

    private void HandleItem(GameSession session, InventoryItemDto item)
    {
        _output.WriteLine();
        _output.WriteLine($"--- {item.DisplayName} ---");

        if (item.CanUse)
            _output.WriteLine("1. Use");
        if (item.CanEquip)
            _output.WriteLine($"{(item.CanUse ? "2" : "1")}. Equip");

        _output.WriteLine("0. Back");

        var max = (item.CanUse ? 1 : 0) + (item.CanEquip ? 1 : 0);
        var choice = _input.ReadChoice("> ", 0, max);

        if (choice == 0) return;

        var actionIndex = 0;
        if (item.CanUse)
        {
            actionIndex++;
            if (choice == actionIndex)
            {
                var result = _useItem.Execute(session, item.ItemId);
                _output.WriteLine(result.Success ? result.Message : result.Error!);
                _output.Pause();
                return;
            }
        }

        if (item.CanEquip)
        {
            actionIndex++;
            if (choice == actionIndex)
            {
                var result = _equipItem.Execute(session, item.ItemId);
                if (result.Success)
                {
                    _output.WriteLine(result.Message);
                    if (result.SwapMessage is not null)
                        _output.WriteLine(result.SwapMessage);
                }
                else
                {
                    _output.WriteLine(result.Error!);
                }

                _output.Pause();
            }
        }
    }
}
