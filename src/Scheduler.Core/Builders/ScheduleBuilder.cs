namespace Scheduler.Core.Builders
{
    using System;

    using NodaTime;

    using Scheduler.Core;

    using Scheduler.Core.Frequencies.Base;

    public class ScheduleBuilder<TFrequency> where TFrequency : Frequency, new()
    {
        private readonly LocalDate _startDate;
        private readonly LocalTime _startTime;
        private readonly LocalTime _endTime;
        private readonly DateTimeZone _timeZone;
        private readonly TFrequency _frequency;

        internal ScheduleBuilder(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            _startDate = startDate;
            _startTime = startTime;
            _endTime = endTime;
            _timeZone = timeZone;
            _frequency = new TFrequency();
        }

        public ScheduleBuilder<TFrequency> Configure(Action<TFrequency> configure)
        {
            configure(_frequency);
            return this;
        }

        public Schedule<TFrequency> Build()
        {
            return new Schedule<TFrequency>
            {
                StartDate = _startDate,
                StartTime = _startTime,
                EndTime = _endTime,
                TimeZone = _timeZone,
                Frequency = _frequency
            };
        }
    }
}
