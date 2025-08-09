namespace Scheduler.Core.Options
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using NodaTime;

    using Scheduler.Core.Contracts;

    public abstract class RecurringOptions : ScheduleOptions
    {
        public int Interval { get; set; } = 1;
    }
}
