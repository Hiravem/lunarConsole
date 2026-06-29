using System.Text.Json;
using Lunar.Core.Application;
using Lunar.Core.Application.Interfaces;

namespace Lunar.Console.Infrastructure;

public sealed class JsonSaveRepository : ISaveRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _savePath;

    public JsonSaveRepository(string? savePath = null)
    {
        _savePath = savePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Lunar",
            "save.json");
    }

    public bool HasSave() => File.Exists(_savePath);

    public void Save(GameState state)
    {
        var directory = Path.GetDirectoryName(_savePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(state, JsonOptions);
        File.WriteAllText(_savePath, json);
    }

    public GameState? Load()
    {
        if (!HasSave())
            return null;

        try
        {
            var json = File.ReadAllText(_savePath);
            return JsonSerializer.Deserialize<GameState>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public void DeleteSave()
    {
        if (HasSave())
            File.Delete(_savePath);
    }

    public string SavePath => _savePath;
}
