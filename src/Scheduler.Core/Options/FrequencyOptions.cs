namespace Scheduler.Core.Options
{
    using NodaTime;
    using Scheduler.Core.Enums;
    using System;

    public abstract class FrequencyOptions
    {
        public int Interval { get; set; } = 1;

        protected internal abstract string GetDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, LocalDate? endDate);

        // Protected helper methods for all frequency options to use
        protected static string GetIndexText(DayOfWeekIndex index)
        {
            switch (index)
            {
                case DayOfWeekIndex.First:
                    return "1st";
                case DayOfWeekIndex.Second:
                    return "2nd";
                case DayOfWeekIndex.Third:
                    return "3rd";
                case DayOfWeekIndex.Fourth:
                    return "4th";
                case DayOfWeekIndex.Last:
                    return "last";
                default:
                    throw new ArgumentException($"Invalid day of week index: {index}");
            }
        }

        protected static string GetDayTypeText(DayOfWeekType dayType)
        {
            switch (dayType)
            {
                case DayOfWeekType.Monday:
                    return "Monday";
                case DayOfWeekType.Tuesday:
                    return "Tuesday";
                case DayOfWeekType.Wednesday:
                    return "Wednesday";
                case DayOfWeekType.Thursday:
                    return "Thursday";
                case DayOfWeekType.Friday:
                    return "Friday";
                case DayOfWeekType.Saturday:
                    return "Saturday";
                case DayOfWeekType.Sunday:
                    return "Sunday";
                case DayOfWeekType.Day:
                    return "day";
                case DayOfWeekType.Weekday:
                    return "weekday";
                case DayOfWeekType.WeekendDay:
                    return "weekend day";
                default:
                    throw new ArgumentException($"Invalid day of week type: {dayType}");
            }
        }

        protected static string GetOrdinal(int day)
        {
            if (day == 1 || day == 21 || day == 31)
                return $"{day}st";
            if (day == 2 || day == 22)
                return $"{day}nd";
            if (day == 3 || day == 23)
                return $"{day}rd";
            return $"{day}th";
        }

        protected static string GetDayName(int dayValue)
        {
            switch (dayValue)
            {
                case 1: return "Monday";
                case 2: return "Tuesday";
                case 3: return "Wednesday";
                case 4: return "Thursday";
                case 5: return "Friday";
                case 6: return "Saturday";
                case 7: return "Sunday";
                default: throw new ArgumentException($"Invalid day value: {dayValue}");
            }
        }

        protected static string GetMonthName(int month)
        {
            switch (month)
            {
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "April";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "September";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
                default: throw new ArgumentException($"Invalid month: {month}");
            }
        }
    }
}