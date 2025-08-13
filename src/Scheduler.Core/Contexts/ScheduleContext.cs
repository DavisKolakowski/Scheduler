namespace Scheduler.Core.Contexts
{
    using System;

    using NodaTime;

    using Scheduler.Core.Builders;
    using Scheduler.Core.Contracts;

    public class ScheduleContext
    {
        private readonly IClock _clock;

        public ScheduleContext(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public ScheduleBuilder CreateBuilder(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            return new ScheduleBuilder(_clock, startDate, startTime, endTime, timeZone);
        }
    }
}
