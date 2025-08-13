namespace Scheduler.Core.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using NodaTime;

    using Scheduler.Core.Contexts;
    using Scheduler.Core.Contracts;
    using Scheduler.Core.Models;
    using Scheduler.Core.Models.Schedules.Base;

    public class RecurringBuilder<TModel> where TModel : Frequency
    {
        private readonly TModel _model;
        private readonly IClock _clock;
        private readonly LocalDate _startDate;
        private readonly LocalTime _startTime;
        private readonly LocalTime _endTime;
        private readonly DateTimeZone _timeZone;
        private readonly LocalDate? _endDate;

        internal RecurringBuilder(
            TModel model,
            IClock clock,
            LocalDate startDate,
            LocalTime startTime,
            LocalTime endTime,
            DateTimeZone timeZone,
            LocalDate? endDate)
        {
            _model = model;
            _clock = clock;
            _startDate = startDate;
            _startTime = startTime;
            _endTime = endTime;
            _timeZone = timeZone;
            _endDate = endDate;
        }

        public ISchedule<TModel> Build()
        {
            _model.StartDate = _startDate;
            _model.StartTime = _startTime;
            _model.EndTime = _endTime;
            _model.EndDate = _endDate;
            _model.TimeZone = _timeZone;

            return new ScheduleContext<TModel>(_model, _clock);
        }
    }
}
