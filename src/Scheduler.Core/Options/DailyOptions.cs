namespace Scheduler.Core.Options
{
    using NodaTime;
    using System;

    public class DailyOptions : FrequencyOptions
    {
        protected internal override string GetDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, LocalDate? endDate)
        {
            var timeRange = $"{startTime:h:mm tt}-{endTime:h:mm tt}";
            var intervalText = Interval == 1 ? "daily" : $"every {Interval} days";
            var endText = endDate.HasValue ? $" until {endDate.Value:MMMM d, yyyy}" : "";
            
            return $"Occurs {intervalText} at {timeRange} starting on {startDate:MMMM d, yyyy}{endText}";
        }
    }
}