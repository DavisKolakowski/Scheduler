using System;

using NodaTime;

using Scheduler.Core.Builders;
using Scheduler.Core.Contracts;
using Scheduler.Core.Options;

namespace Scheduler.Core
{
    public class ScheduleContext
    {
        private readonly CalendarSystem _calendar;
        private readonly IClock _clock;

        public ScheduleContext(IClock? clock = null, CalendarSystem? calendar = null)
        {
            _clock = clock ?? SystemClock.Instance;
            _calendar = calendar ?? CalendarSystem.Iso;
        }

        public ScheduleBuilder CreateBuilder(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            var contextOptions = new ScheduleContextOptions
            {
                StartDate = startDate,
                StartTime = startTime,
                EndTime = endTime,
                TimeZone = timeZone,
                Calendar = _calendar,
                Clock = _clock
            };

            return new ScheduleBuilder(contextOptions);
        }
    }
}