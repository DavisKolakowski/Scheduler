namespace Scheduler.Core
{
    using Scheduler.Core.Options;
    using System;

    public class FrequencyOptionsBuilder
    {
        private FrequencyOptions? _frequencyOptions;

        public FrequencyOptionsBuilder UseDaily(Action<DailyOptions>? configure = null)
        {
            var options = new DailyOptions();
            configure?.Invoke(options);
            _frequencyOptions = options;
            return this;
        }

        public FrequencyOptionsBuilder UseWeekly(Action<WeeklyOptions>? configure = null)
        {
            var options = new WeeklyOptions();
            configure?.Invoke(options);
            _frequencyOptions = options;
            return this;
        }

        public FrequencyOptionsBuilder UseMonthly(Action<MonthlyOptions>? configure = null)
        {
            var options = new MonthlyOptions();
            configure?.Invoke(options);
            _frequencyOptions = options;
            return this;
        }

        public FrequencyOptionsBuilder UseYearly(Action<YearlyOptions>? configure = null)
        {
            var options = new YearlyOptions();
            configure?.Invoke(options);
            _frequencyOptions = options;
            return this;
        }

        public FrequencyOptions Build()
        {
            if (_frequencyOptions == null)
                throw new InvalidOperationException("No frequency option has been configured.");
            
            return _frequencyOptions;
        }
    }
}