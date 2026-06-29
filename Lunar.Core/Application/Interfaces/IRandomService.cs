namespace Lunar.Core.Application.Interfaces;

public interface IRandomService
{
    int Next(int maxExclusive);
    int Next(int minInclusive, int maxExclusive);
}
