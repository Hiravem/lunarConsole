using Lunar.Core.Application.Interfaces;
using Lunar.Core.Domain.Characters;
using Lunar.Core.Domain.Combat;

namespace Lunar.Core.Domain.World;

public sealed class EnemyFactory
{
    public Enemy Create(string enemyId, int difficulty)
    {
        var scale = 1 + (difficulty - 1) * 0.15;

        return enemyId switch
        {
            "goblin" => new Enemy(
                "goblin", "Goblin",
                new Health((int)(40 * scale)),
                new Stats((int)(8 * scale), (int)(2 * scale)),
                "goblin_loot",
                SimpleEnemyAI.Instance),
            "skeleton" => new Enemy(
                "skeleton", "Skeleton",
                new Health((int)(55 * scale)),
                new Stats((int)(10 * scale), (int)(4 * scale)),
                "skeleton_loot",
                SimpleEnemyAI.Instance),
            _ => Create("goblin", difficulty)
        };
    }
}
