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

        public FrequencyScheduleBuilder<DailyOptions> Daily(Action<DailyOptions>? configure = null)
        {
            var options = new DailyOptions();
            configure?.Invoke(options);
            return new FrequencyScheduleBuilder<DailyOptions>(options, _contextOptions);
        }

        public FrequencyScheduleBuilder<WeeklyOptions> Weekly(Action<WeeklyOptions>? configure = null)
        {
            var options = new WeeklyOptions();
            options.Initialize((int)_contextOptions.StartDate.DayOfWeek);
            configure?.Invoke(options);
            return new FrequencyScheduleBuilder<WeeklyOptions>(options, _contextOptions);
        }

        public FrequencyScheduleBuilder<MonthlyOptions> Monthly(Action<MonthlyOptions>? configure = null)
        {
            var options = new MonthlyOptions();
            options.Initialize(_contextOptions.StartDate.Day);
            configure?.Invoke(options);
            return new FrequencyScheduleBuilder<MonthlyOptions>(options, _contextOptions);
        }

        public FrequencyScheduleBuilder<YearlyOptions> Yearly(Action<YearlyOptions>? configure = null)
        {
            var options = new YearlyOptions();
            options.Initialize(_contextOptions.StartDate);
            configure?.Invoke(options);
            return new FrequencyScheduleBuilder<YearlyOptions>(options, _contextOptions);
        }
    }
}
