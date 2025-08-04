using NodaTime;
using Scheduler.Core.Contracts;
using Scheduler.Core.Enums;

namespace Scheduler.Core.Frequencies.Base
{
    public abstract class Recurring : IFrequency
    {
        public int Interval { get; set; } = 1;
        public virtual LocalDate? ExpirationDate { get; set; }
        
        public bool IsExpired(LocalDate currentDate)
        {
            return ExpirationDate.HasValue && currentDate > ExpirationDate.Value;
        }
        
        public abstract string GenerateDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone);
        
        protected static string GetDayName(int dayNumber)
        {
            return dayNumber switch
            {
                1 => "Monday",
                2 => "Tuesday", 
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                7 => "Sunday",
                _ => dayNumber.ToString()
            };
        }
        
        protected static string GetDayName(DayOfWeekType dayOfWeekType)
        {
            return dayOfWeekType switch
            {
                DayOfWeekType.Monday => "Monday",
                DayOfWeekType.Tuesday => "Tuesday", 
                DayOfWeekType.Wednesday => "Wednesday",
                DayOfWeekType.Thursday => "Thursday",
                DayOfWeekType.Friday => "Friday",
                DayOfWeekType.Saturday => "Saturday",
                DayOfWeekType.Sunday => "Sunday",
                DayOfWeekType.Day => "day",
                DayOfWeekType.Weekday => "weekday",
                DayOfWeekType.WeekendDay => "weekend day",
                _ => dayOfWeekType.ToString()
            };
        }
        
        protected static string GetMonthName(int monthNumber)
        {
            return monthNumber switch
            {
                1 => "January",
                2 => "February",
                3 => "March", 
                4 => "April",
                5 => "May",
                6 => "June",
                7 => "July",
                8 => "August",
                9 => "September",
                10 => "October",
                11 => "November",
                12 => "December",
                _ => monthNumber.ToString()
            };
        }
        
        protected static string GetOrdinalSuffix(int number)
        {
            if (number >= 11 && number <= 13) return "th";
            
            return (number % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }
    }
}