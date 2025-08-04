using Scheduler.Core.Frequencies.Base;

namespace Scheduler.Core.Frequencies.Base
{
    public abstract class Recurring : Frequency
    {
        public int Interval { get; set; } = 1;
    }
}