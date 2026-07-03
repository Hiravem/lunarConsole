using Lunar.Core.Model;

namespace Lunar.Core.Repository;

public interface ISaveRepository
{
    bool HasSave();
    void Save(GameState state);
    GameState? Load();
    void DeleteSave();
}
