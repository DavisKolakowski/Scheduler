namespace Scheduler.Core.Builders
{
    using Scheduler.Core.Contracts;
    using Scheduler.Core.Models.Schedules;

    public class OneTimeBuilder
    {
        private readonly RecurringBuilder<OneTime> _builder;

        internal OneTimeBuilder(RecurringBuilder<OneTime> builder)
        {
            _builder = builder;
        }

        public ISchedule<OneTime> Build()
        {
            return _builder.Build();
        }
    }
}
