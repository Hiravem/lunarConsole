using Lunar.Core.Model;
using Lunar.Core.Service;

namespace Lunar.Console.UI.Screens;

public sealed class EquipmentScreen
{
    private readonly OutputWriter _output;

    public EquipmentScreen(OutputWriter output) => _output = output;

    public void Show(GameSession session)
    {
        var dto = GameplayPresenterMapper.ToEquipmentDto(session.Player);

        _output.WriteLine();
        _output.WriteHeader("Equipment");
        _output.WriteLine($"Weapon : {dto.WeaponName}");
        _output.WriteLine($"Armor  : {dto.ArmorName}");
        _output.WriteLine($"Ring   : {dto.RingName}");
        _output.WriteSeparator('-');
        _output.WriteLine($"Effective ATK: {dto.EffectiveAttack}");
        _output.WriteLine($"Effective DEF: {dto.EffectiveDefense}");
        _output.WriteLine();
        _output.WriteLine("Equip items from Inventory.");
        _output.Pause();
    }
}
