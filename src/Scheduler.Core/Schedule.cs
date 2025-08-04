using System;

using NodaTime;

using Scheduler.Core.Builders;
using Scheduler.Core.Frequencies.Base;

namespace Scheduler.Core
{
    public class Schedule<TFrequency> where TFrequency : Frequency, new()
    {
        public LocalDate StartDate { get; internal set; }
        public LocalTime StartTime { get; internal set; }
        public LocalTime EndTime { get; internal set; }
        public LocalDate? ExpirationDate => Frequency.ExpirationDate;
        public DateTimeZone TimeZone { get; internal set; } = null!;
        public TFrequency Frequency { get; internal set; } = new TFrequency();
             
        public bool IsExpired(LocalDate currentDate) => Frequency.IsExpired(currentDate);
        
        public static ScheduleBuilder<TFrequency> CreateBuilder(
            LocalDate startDate,
            LocalTime startTime, 
            LocalTime endTime,
            DateTimeZone timeZone)
        {
            return new ScheduleBuilder<TFrequency>(startDate, startTime, endTime, timeZone);
        }
        
        public override string ToString()
        {
            return Frequency.GetScheduleDescription(StartDate, StartTime, EndTime, TimeZone);
        }
    }
}