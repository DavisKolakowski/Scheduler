namespace Scheduler.Core.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Scheduler.Core.Options;

    public class RecurringScheduleBuilder
    {
        private readonly ScheduleContextOptions _contextOptions;

        internal RecurringScheduleBuilder(ScheduleContextOptions contextOptions)
        {
            _contextOptions = contextOptions;
        }

        public RecurringFrequencyOptionsBuilder<DailyOptions> Daily(Action<DailyOptions>? configure = null)
        {
            var options = new DailyOptions();
            configure?.Invoke(options);
            return new RecurringFrequencyOptionsBuilder<DailyOptions>(options, _contextOptions);
        }

        public RecurringFrequencyOptionsBuilder<WeeklyOptions> Weekly(Action<WeeklyOptions>? configure = null)
        {
            var options = new WeeklyOptions();
            options.Initialize((int)_contextOptions.StartDate.DayOfWeek);
            configure?.Invoke(options);
            return new RecurringFrequencyOptionsBuilder<WeeklyOptions>(options, _contextOptions);
        }

        public RecurringFrequencyOptionsBuilder<MonthlyOptions> Monthly(Action<MonthlyOptions>? configure = null)
        {
            var options = new MonthlyOptions();
            options.Initialize(_contextOptions.StartDate.Day);
            configure?.Invoke(options);
            return new RecurringFrequencyOptionsBuilder<MonthlyOptions>(options, _contextOptions);
        }

        public RecurringFrequencyOptionsBuilder<YearlyOptions> Yearly(Action<YearlyOptions>? configure = null)
        {
            var options = new YearlyOptions();
            options.Initialize(_contextOptions.StartDate);
            configure?.Invoke(options);
            return new RecurringFrequencyOptionsBuilder<YearlyOptions>(options, _contextOptions);
        }
    }
}
