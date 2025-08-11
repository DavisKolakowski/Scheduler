namespace Scheduler.Core.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    using Scheduler.Core.Contracts;
    using Scheduler.Core.Models;
    using Scheduler.Core.Options;

    public class RecurringFrequencyOptionsBuilder<T> where T : RecurringOptions
    {
        private readonly T _options;
        private readonly ScheduleContextOptions _contextOptions;

        internal RecurringFrequencyOptionsBuilder(T options, ScheduleContextOptions contextOptions)
        {
            _options = options;
            _contextOptions = contextOptions;
        }

        public ISchedule<T> Build()
        {
            _options.StartDate = _contextOptions.StartDate;
            _options.StartTime = _contextOptions.StartTime;
            _options.EndTime = _contextOptions.EndTime;
            _options.EndDate = _contextOptions.EndDate;
            _options.TimeZone = _contextOptions.TimeZone;

            return new Schedule<T>(_options, _contextOptions.Clock);
        }
    }
}
