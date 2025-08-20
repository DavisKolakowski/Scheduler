namespace Scheduler.Core.Builders
{
    using System;
    using NodaTime;
    using Scheduler.Core.Models.Frequencies;

    public class ScheduleBuilder
    {
        private readonly IClock _clock;
        private readonly LocalDate _startDate;
        private readonly LocalTime _startTime;
        private readonly LocalTime _endTime;
        private readonly DateTimeZone _timeZone;

        internal ScheduleBuilder(
            IClock clock,
            LocalDate startDate,
            LocalTime startTime,
            LocalTime endTime,
            DateTimeZone timeZone)
        {
            _clock = clock;
            _startDate = startDate;
            _startTime = startTime;
            _endTime = endTime;
            _timeZone = timeZone;
        }

        public FrequencyBuilder<Once> OneTime()
        {
            var model = new Once();
            LocalDate endDate = _endTime <= _startTime ? _startDate.PlusDays(1) : _startDate;
            return new FrequencyBuilder<Once>(model, _clock, _startDate, _startTime, _endTime, _timeZone, endDate);
        }

        public FrequencyBuilder<Daily> Daily(Action<Daily>? configure = null)
        {
            var model = new Daily();
            configure?.Invoke(model);
            return new FrequencyBuilder<Daily>(model, _clock, _startDate, _startTime, _endTime, _timeZone, model.EndDate);
        }

        public FrequencyBuilder<Weekly> Weekly(Action<Weekly>? configure = null)
        {
            var model = new Weekly();
            model.Initialize((int)_startDate.DayOfWeek);
            configure?.Invoke(model);
            return new FrequencyBuilder<Weekly>(model, _clock, _startDate, _startTime, _endTime, _timeZone, model.EndDate);
        }

        public FrequencyBuilder<Monthly> Monthly(Action<Monthly>? configure = null)
        {
            var model = new Monthly();
            model.Initialize(_startDate.Day);
            configure?.Invoke(model);
            return new FrequencyBuilder<Monthly>(model, _clock, _startDate, _startTime, _endTime, _timeZone, model.EndDate);
        }

        public FrequencyBuilder<Yearly> Yearly(Action<Yearly>? configure = null)
        {
            var model = new Yearly();
            model.Initialize(_startDate);
            configure?.Invoke(model);
            return new FrequencyBuilder<Yearly>(model, _clock, _startDate, _startTime, _endTime, _timeZone, model.EndDate);
        }
    }
}
