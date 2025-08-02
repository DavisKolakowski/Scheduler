namespace Scheduler.Core.Options
{
    using NodaTime;
    using Scheduler.Core.Enums;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class YearlyOptions : FrequencyOptions
    {
        public bool UseRelative { get; private set; }
        public RelativeYearlyOptions? RelativeOptions { get; private set; }
        public DayOfWeekIndex? WeekIndex { get; private set; }
        public DayOfWeekType? WeekDayType { get; private set; }

        public YearlyOptions UseRelativeYearly(Action<RelativeYearlyOptions> configure)
        {
            UseRelative = true;
            RelativeOptions = new RelativeYearlyOptions();
            configure(RelativeOptions);
            return this;
        }

        public YearlyOptions OnDaysOfWeek(DayOfWeekIndex dayOfWeekIndex, DayOfWeekType dayOfWeekType)
        {
            WeekIndex = dayOfWeekIndex;
            WeekDayType = dayOfWeekType;
            return this;
        }

        protected internal override string GetDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, LocalDate? endDate)
        {
            var timeRange = $"{startTime:h:mm tt}-{endTime:h:mm tt}";
            var intervalText = Interval == 1 ? "yearly" : $"every {Interval} years";
            var endText = endDate.HasValue ? $" until {endDate.Value:MMMM d, yyyy}" : "";

            string dateText;
            if (UseRelative && RelativeOptions != null)
            {
                var monthNames = RelativeOptions.Months.Select(GetMonthName).ToList();
                string monthText;
                if (monthNames.Count == 1)
                {
                    monthText = $"in {monthNames[0]}";
                }
                else if (monthNames.Count == 2)
                {
                    monthText = $"in {monthNames[0]}, {monthNames[1]}";
                }
                else
                {
                    monthText = $"in {string.Join(", ", monthNames.Take(monthNames.Count - 1))}, {monthNames.Last()}";
                }

                if (WeekIndex.HasValue && WeekDayType.HasValue)
                {
                    var indexText = GetIndexText(WeekIndex.Value);
                    var dayTypeText = GetDayTypeText(WeekDayType.Value);
                    dateText = $"{monthText} on the {indexText} {dayTypeText}";
                }
                else
                {
                    // Use the same date from start date
                    var ordinal = GetOrdinal(startDate.Day);
                    dateText = $"{monthText} on the {ordinal}";
                }
            }
            else
            {
                // Absolute date - same date each year
                dateText = "";
            }

            return $"Occurs {intervalText}{(string.IsNullOrEmpty(dateText) ? "" : " " + dateText)} at {timeRange} starting on {startDate:MMMM d, yyyy}{endText}";
        }
    }
}