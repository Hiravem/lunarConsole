using Lunar.Core.Util;

namespace Lunar.Console.Util;

public sealed class RandomService : IRandomService
{
    private readonly Random _random = new();

    public int Next(int maxExclusive) => _random.Next(maxExclusive);

    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
}
