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

        public ScheduleContext(IClock clock)
        {
            _clock = clock;
            _calendar = CalendarSystem.Iso;
        }

        public ScheduleContext(IClock clock, CalendarSystem calendar)
        {
            _clock = clock;
            _calendar = calendar;
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