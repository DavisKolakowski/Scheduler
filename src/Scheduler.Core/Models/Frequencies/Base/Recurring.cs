namespace Scheduler.Core.Models.Frequencies.Base
{
    using System;

    public abstract class Recurring : Frequency
    {
        private int _interval = 1;

        public int Interval
        {
            get => _interval;
            set => _interval = Math.Max(1, value);
        }
    }
}
