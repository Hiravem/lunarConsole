using Lunar.Core.Application.DTOs;
using Lunar.Core.Domain.Characters;
using Lunar.Core.Domain.Items;

namespace Lunar.Core.Application.UseCases;

public sealed class StartGameUseCase
{
    private readonly ItemFactory _itemFactory;

    public StartGameUseCase(ItemFactory itemFactory) => _itemFactory = itemFactory;

    public GameSession Execute()
    {
        var player = new Player(
            name: "Hero",
            health: new Health(120),
            stats: new Stats(attack: 15, defense: 5, critChance: 10),
            itemFactory: _itemFactory,
            gold: 0);

        return GameSession.CreateNew(player);
    }
}
