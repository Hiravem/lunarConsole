using Lunar.Core.Util;

namespace Lunar.Core.Model.World;

public sealed class EncounterTable
{
    private readonly (IExploreEncounter encounter, int weight)[] _entries;

    public EncounterTable(params (IExploreEncounter encounter, int weight)[] entries) =>
        _entries = entries;

    public static EncounterTable Default { get; } = new(
        (new EnemyEncounter("enc_goblin", "goblin", "A goblin blocks your path!"), 35),
        (new EnemyEncounter("enc_skeleton", "skeleton", "A skeleton rises from the dust!"), 25),
        (new ChestEncounter("enc_chest", "chest_loot", "You discover a wooden chest!"), 15),
        (new EmptyEncounter("enc_empty", "The path is quiet. Nothing happens."), 10),
        (new EventEncounter("enc_event", "A mysterious shrine appears..."), 10),
        (new MerchantEncounter("enc_merchant", "A traveling merchant greets you."), 5));

    public IExploreEncounter PickRandom(IRandomService random)
    {
        var total = _entries.Sum(e => e.weight);
        var roll = random.Next(total);
        var cumulative = 0;

        foreach (var (encounter, weight) in _entries)
        {
            cumulative += weight;
            if (roll < cumulative)
                return encounter;
        }

        return _entries[0].encounter;
    }
}
