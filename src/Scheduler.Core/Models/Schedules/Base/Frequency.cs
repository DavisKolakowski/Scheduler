namespace Scheduler.Core.Models.Schedules.Base
{
    using System;

    using NodaTime;

    using Scheduler.Core.Builders;
    using Scheduler.Core.Contracts;

    public abstract class Frequency
    {
        public LocalDate StartDate { get; internal set; }
        public LocalTime StartTime { get; internal set; }
        public LocalTime EndTime { get; internal set; }
        public DateTimeZone TimeZone { get; internal set; } = null!;
        public LocalDate? EndDate { get; internal set; }
    }
}
