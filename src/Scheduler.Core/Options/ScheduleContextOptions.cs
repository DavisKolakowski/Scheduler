namespace Scheduler.Core.Options
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Extensions.Options;

    using NodaTime;

    using Scheduler.Core.Contracts;

    public class ScheduleContextOptions : IScheduleOptions
    {
        public LocalDate StartDate { get; set; }
        public LocalTime StartTime { get; set; }
        public LocalTime EndTime { get; set; }
        public DateTimeZone TimeZone { get; set; } = null!;
        public IClock Clock { get; set; } = null!;
        public LocalDate? EndDate { get; set; }
        public CalendarSystem CalendarSystem { get; set; } = CalendarSystem.Iso;
    }
}
