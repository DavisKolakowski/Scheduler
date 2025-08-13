namespace Scheduler.Core.Builders
{
    using System;
    using NodaTime;
    using Scheduler.Core.Contracts;
    using Scheduler.Core.Models.Schedules;

    public class ScheduleBuilder
    {
        private readonly IClock _clock;
        private readonly LocalDate _startDate;
        private readonly LocalTime _startTime;
        private readonly LocalTime _endTime;
        private readonly DateTimeZone _timeZone;
        private readonly LocalDate? _endDate;

        internal ScheduleBuilder(
            IClock clock,
            LocalDate startDate,
            LocalTime startTime,
            LocalTime endTime,
            DateTimeZone timeZone,
            LocalDate? endDate)
        {
            _clock = clock;
            _startDate = startDate;
            _startTime = startTime;
            _endTime = endTime;
            _timeZone = timeZone;
            _endDate = endDate;
        }

        public OneTimeBuilder OneTime()
        {
            var model = new OneTime();
            var builder = new RecurringBuilder<OneTime>(model, _clock, _startDate, _startTime, _endTime, _timeZone, _endDate);
            return new OneTimeBuilder(builder);
        }

        public RecurringBuilder<Daily> Daily(Action<Daily>? configure = null)
        {
            var model = new Daily();
            configure?.Invoke(model);
            return new RecurringBuilder<Daily>(model, _clock, _startDate, _startTime, _endTime, _timeZone, _endDate);
        }

        public RecurringBuilder<Weekly> Weekly(Action<Weekly>? configure = null)
        {
            var model = new Weekly();
            model.Initialize((int)_startDate.DayOfWeek);
            configure?.Invoke(model);
            return new RecurringBuilder<Weekly>(model, _clock, _startDate, _startTime, _endTime, _timeZone, _endDate);
        }

        public RecurringBuilder<Monthly> Monthly(Action<Monthly>? configure = null)
        {
            var model = new Monthly();
            model.Initialize(_startDate.Day);
            configure?.Invoke(model);
            return new RecurringBuilder<Monthly>(model, _clock, _startDate, _startTime, _endTime, _timeZone, _endDate);
        }

        public RecurringBuilder<Yearly> Yearly(Action<Yearly>? configure = null)
        {
            var model = new Yearly();
            model.Initialize(_startDate);
            configure?.Invoke(model);
            return new RecurringBuilder<Yearly>(model, _clock, _startDate, _startTime, _endTime, _timeZone, _endDate);
        }
    }
}
