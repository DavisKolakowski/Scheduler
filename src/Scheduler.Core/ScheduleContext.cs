using System;

using NodaTime;

using Scheduler.Core.Builders;
using Scheduler.Core.Contracts;
using Scheduler.Core.Options;

namespace Scheduler.Core
{
    public class ScheduleContext
    {
        private readonly IClock _clock;

        public ScheduleContext(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public ScheduleContextOptionsBuilder CreateBuilder(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            var contextOptions = new ScheduleContextOptions
            {
                StartDate = startDate,
                StartTime = startTime,
                EndTime = endTime,
                TimeZone = timeZone,
                Clock = _clock
            };

            return new ScheduleContextOptionsBuilder(contextOptions);
        }
    }
}