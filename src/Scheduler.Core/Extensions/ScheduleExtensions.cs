namespace Scheduler.Core.Extensions
{
    using NodaTime;
    using System;

    public static class ScheduleExtensions
    {
        /// <summary>
        /// Convenience method to create a LocalDate
        /// </summary>
        public static LocalDate LocalDate(int year, int month, int day)
        {
            return new LocalDate(year, month, day);
        }

        /// <summary>
        /// Convenience method to create a LocalTime
        /// </summary>
        public static LocalTime LocalTime(int hour, int minute)
        {
            return new LocalTime(hour, minute);
        }

        /// <summary>
        /// Convenience method to create a LocalTime with seconds
        /// </summary>
        public static LocalTime LocalTime(int hour, int minute, int second)
        {
            return new LocalTime(hour, minute, second);
        }
    }
}
