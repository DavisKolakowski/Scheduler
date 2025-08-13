namespace Scheduler.Core.Factories
{
    using System;

    using NodaTime;

    using Scheduler.Core.Builders;
    using Scheduler.Core.Contracts;

    public class ScheduleFactory
    {
        private readonly IClock _clock;

        public ScheduleFactory(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public ScheduleContextOptionsBuilder Create(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone, LocalDate? endDate = null)
        {
            return new ScheduleContextOptionsBuilder(_clock, startDate, startTime, endTime, timeZone, endDate);
        }
    }
}
