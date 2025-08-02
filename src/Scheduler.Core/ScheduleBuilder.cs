namespace Scheduler.Core
{
    using NodaTime;
    using Scheduler.Core.Options;
    using System;

    public class ScheduleBuilder
    {
        private readonly LocalDate _startDate;
        private readonly LocalTime _startTime;
        private readonly LocalTime _endTime;
        private readonly DateTimeZone _timeZone;
        private LocalDate? _endDate;
        private FrequencyOptions? _recurrence;

        internal ScheduleBuilder(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            _startDate = startDate;
            _startTime = startTime;
            _endTime = endTime;
            _timeZone = timeZone;
        }

        public ScheduleBuilder AddEndDate(LocalDate endDate)
        {
            _endDate = endDate;
            return this;
        }

        public ScheduleBuilder AddRecurrence(Action<FrequencyOptionsBuilder> configure)
        {
            var builder = new FrequencyOptionsBuilder();
            configure(builder);
            _recurrence = builder.Build();
            return this;
        }

        public Schedule Build()
        {
            return new Schedule(_startDate, _startTime, _endTime, _timeZone, _endDate, _recurrence);
        }
    }
}