namespace Scheduler.Core.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using NodaTime;

    public interface IScheduleOptions
    {
        LocalDate StartDate { get; set; }
        LocalTime StartTime { get; set; }
        LocalTime EndTime { get; set; }
        DateTimeZone TimeZone { get; set; }
        LocalDate? EndDate { get; set; }
        CalendarSystem CalendarSystem { get; set; }
    }
}
