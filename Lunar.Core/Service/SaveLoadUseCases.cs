using Lunar.Core.Model;
using Lunar.Core.Model.Dto;
using Lunar.Core.Model.Events;
using Lunar.Core.Model.Items;
using Lunar.Core.Repository;
using Lunar.Core.Util;

namespace Lunar.Core.Service;

public sealed class SaveGameUseCase
{
    private readonly ISaveRepository _saveRepository;
    private readonly IEventBus _eventBus;

    public SaveGameUseCase(ISaveRepository saveRepository, IEventBus eventBus)
    {
        _saveRepository = saveRepository;
        _eventBus = eventBus;
    }

    public SaveResultDto Execute(GameSession session, bool silent = false)
    {
        if (session.ActiveCombatSession is not null)
            return SaveResultDto.Fail("Cannot save during combat.");

        if (session.Player.Health.IsDead)
            return SaveResultDto.Fail("Cannot save — player is dead.");

        var state = GameState.FromSession(session);
        _saveRepository.Save(state);
        _eventBus.Publish(new GameSaved(session.CurrentDay, session.Difficulty));

        return SaveResultDto.Ok(silent ? "" : $"Game saved. (Day {session.CurrentDay}, Difficulty {session.Difficulty})");
    }
}

public sealed class LoadGameUseCase
{
    private readonly ISaveRepository _saveRepository;
    private readonly ItemFactory _itemFactory;

    public LoadGameUseCase(ISaveRepository saveRepository, ItemFactory itemFactory)
    {
        _saveRepository = saveRepository;
        _itemFactory = itemFactory;
    }

    public LoadResultDto Execute()
    {
        if (!_saveRepository.HasSave())
            return LoadResultDto.Fail("No save file found. Start a new game first.");

        var state = _saveRepository.Load();
        if (state is null)
            return LoadResultDto.Fail("Save file is corrupted or unreadable.");

        if (state.SchemaVersion > GameState.CurrentSchemaVersion)
            return LoadResultDto.Fail("Save file requires a newer version of the game.");

        var session = state.ToSession(_itemFactory);
        return LoadResultDto.Ok(session, $"Welcome back! Day {session.CurrentDay}, Difficulty {session.Difficulty}.");
    }
}
