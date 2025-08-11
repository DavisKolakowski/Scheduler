namespace Scheduler.Core.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using NodaTime;

    using Scheduler.Core.Contracts;
    using Scheduler.Core.Models;
    using Scheduler.Core.Options;

    public class ScheduleContextOptionsBuilder
    {
        private readonly ScheduleContextOptions _contextOptions;

        internal ScheduleContextOptionsBuilder(ScheduleContextOptions contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public RecurringScheduleBuilder Recurring(LocalDate? endDate = null)
        {
            _contextOptions.EndDate = endDate;
            return new RecurringScheduleBuilder(_contextOptions);
        }

        public ISchedule<OneTimeOptions> Build()
        {
            var options = new OneTimeOptions
            {
                StartDate = _contextOptions.StartDate,
                StartTime = _contextOptions.StartTime,
                EndTime = _contextOptions.EndTime,
                TimeZone = _contextOptions.TimeZone
            };

            return new Schedule<OneTimeOptions>(options, _contextOptions.Clock);
        }
    }
}
