namespace Scheduler.Core
{
    using NodaTime;
    using Scheduler.Core.Options;
    using System;

    public class Schedule
    {
        public LocalDate StartDate { get; }
        public LocalTime StartTime { get; }
        public LocalTime EndTime { get; }    
        public LocalDate? EndDate { get; }
        public DateTimeZone TimeZone { get; }
        public FrequencyOptions? Recurrence { get; }

        internal Schedule(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone, LocalDate? endDate = null, FrequencyOptions? recurrence = null)
        {
            StartDate = startDate;
            StartTime = startTime;
            EndTime = endTime;        
            EndDate = endDate;
            TimeZone = timeZone;
            Recurrence = recurrence;
        }

        public static ScheduleBuilder CreateBuilder(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            return new ScheduleBuilder(startDate, startTime, endTime, timeZone);
        }

        public override string ToString()
        {
            // Implementation for generating the description based on the schedule configuration
            // This would be implemented based on the recurrence pattern
            if (Recurrence == null)
            {
                var occurrence = EndDate.HasValue && EndDate.Value == StartDate ? "once" : "once";
                return $"Occurs {occurrence} on {StartDate:MMMM d, yyyy} at {StartTime:h:mm tt}-{EndTime:h:mm tt}";
            }

            // Handle recurrence patterns
            return Recurrence.GetDescription(StartDate, StartTime, EndTime, EndDate);
        }
    }
}