namespace Scheduler.Core.Options
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using NodaTime;

    using Scheduler.Core.Contracts;

    public abstract class RecurringOptions : ScheduleOptions
    {
        private int _interval = 1;

        public int Interval 
        { 
            get => _interval;
            set => _interval = Math.Max(1, value);
        }
    }
}
