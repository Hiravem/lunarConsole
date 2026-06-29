namespace Lunar.Core.Application;

public sealed class MetaProgression
{
    public int BossesDefeated { get; set; }
    public int HighestDifficultyReached { get; set; } = 1;

    public void OnBossDefeated(int newDifficulty)
    {
        BossesDefeated++;
        if (newDifficulty > HighestDifficultyReached)
            HighestDifficultyReached = newDifficulty;
    }
}
