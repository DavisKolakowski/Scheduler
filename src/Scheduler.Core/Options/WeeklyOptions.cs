namespace Scheduler.Core.Options
{
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WeeklyOptions : FrequencyOptions
    {
        public List<int> Days { get; set; } = new List<int>();

        public override string GetDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, LocalDate? endDate)
        {
            var timeRange = $"{startTime:h:mm tt}-{endTime:h:mm tt}";
            var intervalText = Interval == 1 ? "weekly" : $"every {Interval} weeks";
            var endText = endDate.HasValue ? $" until {endDate.Value:MMMM d, yyyy}" : "";

            string dayText;
            if (Days.Count == 0)
            {
                // Default to the start date's day of week
                var dayOfWeek = startDate.DayOfWeek;
                dayText = $"on {GetDayName((int)dayOfWeek)}";
            }
            else
            {
                var dayNames = Days.Select(GetDayName).ToList();
                if (dayNames.Count == 1)
                {
                    dayText = $"on {dayNames[0]}";
                }
                else if (dayNames.Count == 2)
                {
                    dayText = $"on {dayNames[0]}, {dayNames[1]}";
                }
                else
                {
                    dayText = $"on {string.Join(", ", dayNames.Take(dayNames.Count - 1))}, {dayNames.Last()}";
                }
            }

            return $"Occurs {intervalText} {dayText} at {timeRange} starting on {startDate:MMMM d, yyyy}{endText}";
        }
    }
}