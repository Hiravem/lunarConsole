namespace Lunar.Core.Application.Interfaces;

public interface ISaveRepository
{
    bool HasSave();
    void Save(GameState state);
    GameState? Load();
    void DeleteSave();
}
