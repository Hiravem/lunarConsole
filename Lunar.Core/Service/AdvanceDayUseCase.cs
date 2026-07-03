using Lunar.Core.Model;
using Lunar.Core.Model.Dto;
using Lunar.Core.Util;
using Lunar.Core.Model.Characters;
using Lunar.Core.Model.Events;

namespace Lunar.Core.Service;

public sealed class AdvanceDayUseCase
{
    private readonly IEventBus _eventBus;

    public AdvanceDayUseCase(IEventBus eventBus) => _eventBus = eventBus;

    public AdvanceDayResultDto Execute(GameSession session)
    {
        if (!session.DayFlags.HasUsedDayAction && !session.IsBossDay)
            return new AdvanceDayResultDto
            {
                Success = false,
                NewDay = session.CurrentDay,
                Message = "Use Explore or Rest before advancing to the next day."
            };

        if (session.IsBossDay)
            return new AdvanceDayResultDto
            {
                Success = false,
                NewDay = session.CurrentDay,
                Message = "Defeat the boss before advancing!"
            };

        session.DayFlags.Reset();
        session.CurrentDay++;
        session.IsBossDay = session.CurrentDay % GameBalance.BossIntervalDays == 0;

        _eventBus.Publish(new DayAdvanced(session.CurrentDay));

        var message = session.IsBossDay
            ? $"Day {session.CurrentDay} — The boss awaits!"
            : $"Day {session.CurrentDay} begins.";

        return new AdvanceDayResultDto
        {
            Success = true,
            NewDay = session.CurrentDay,
            IsBossDay = session.IsBossDay,
            Message = message
        };
    }
}
