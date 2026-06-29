using Lunar.Core.Application.DTOs;

namespace Lunar.Core.Application.UseCases;

public sealed class RestUseCase
{
    public RestResultDto Execute(GameSession session)
    {
        if (session.IsBossDay)
            return Fail("Boss day! No time to rest.");

        if (session.DayFlags.HasUsedDayAction)
            return Fail("You already used your main action today (explore or rest).");

        session.DayFlags.HasRested = true;
        var healed = session.Player.Rest();

        return new RestResultDto
        {
            Success = true,
            HealedAmount = healed
        };
    }

    private static RestResultDto Fail(string error) =>
        new() { Success = false, Error = error };
}
