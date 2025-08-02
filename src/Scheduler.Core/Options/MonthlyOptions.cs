namespace Scheduler.Core.Options
{
    using NodaTime;
    using Scheduler.Core.Enums;
    using System;

    public class MonthlyOptions : FrequencyOptions
    {
        public bool UseRelative { get; private set; }
        public DayOfWeekIndex? WeekIndex { get; private set; }
        public DayOfWeekType? WeekDayType { get; private set; }

        public MonthlyOptions UseRelativeMonthly(DayOfWeekIndex dayOfWeekIndex, DayOfWeekType dayOfWeekType)
        {
            UseRelative = true;
            WeekIndex = dayOfWeekIndex;
            WeekDayType = dayOfWeekType;
            return this;
        }

        protected internal override string GetDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, LocalDate? endDate)
        {
            var timeRange = $"{startTime:h:mm tt}-{endTime:h:mm tt}";
            var intervalText = Interval == 1 ? "monthly" : $"every {Interval} months";
            var endText = endDate.HasValue ? $" until {endDate.Value:MMMM d, yyyy}" : "";

            string dateText;
            if (UseRelative && WeekIndex.HasValue && WeekDayType.HasValue)
            {
                var indexText = GetIndexText(WeekIndex.Value);
                var dayTypeText = GetDayTypeText(WeekDayType.Value);
                dateText = $"on the {indexText} {dayTypeText}";
            }
            else
            {
                // Absolute date - use the day from start date
                var ordinal = GetOrdinal(startDate.Day);
                dateText = $"on the {ordinal}";
            }

            return $"Occurs {intervalText} {dateText} at {timeRange} starting on {startDate:MMMM d, yyyy}{endText}";
        }
    }
}