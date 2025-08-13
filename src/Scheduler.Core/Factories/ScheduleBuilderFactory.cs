namespace Scheduler.Core.Factories
{
    using System;

    using NodaTime;

    using Scheduler.Core.Builders;
    using Scheduler.Core.Contracts;

    public class ScheduleBuilderFactory
    {
        private readonly IClock _clock;

        public ScheduleBuilderFactory(IClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public ScheduleBuilder Create(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone, LocalDate? endDate = null)
        {
            return new ScheduleBuilder(_clock, startDate, startTime, endTime, timeZone, endDate);
        }
    }
}
