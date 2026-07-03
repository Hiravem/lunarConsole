namespace Lunar.Core.Util;

public interface IRandomService
{
    int Next(int maxExclusive);
    int Next(int minInclusive, int maxExclusive);
}
