using Scheduler.Core.Frequencies.Base;
using Scheduler.Core.Enums;
using NodaTime;
using Scheduler.Core.Options;

namespace Scheduler.Core.Frequencies
{
    public class Monthly : Recurring
    {
        public RelativeOptions? RelativeOptions { get; set; }
        public int DayOfMonth { get; set; }
        
        public void UseRelative(DayOfWeekIndex relativeIndex, DayOfWeekType relativeDayOfWeek)
        {
            RelativeOptions = new RelativeOptions(relativeIndex, relativeDayOfWeek);
        }
        
        public override string GetScheduleDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            var startTimeFormatted = startTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var endTimeFormatted = endTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var startDateFormatted = startDate.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
            
            var intervalText = Interval == 1 ? "every month" : $"every {Interval} months";
            var dayText = GetDayText(startDate);
            
            var description = $"Occurs {intervalText}{dayText} at {startTimeFormatted}-{endTimeFormatted} ({timeZone.Id}) starting on {startDateFormatted}";
            
            if (ExpirationDate.HasValue)
            {
                var expirationFormatted = ExpirationDate.Value.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
                description += $" until {expirationFormatted}";
            }
            
            return description;
        }
        
        private string GetDayText(LocalDate startDate)
        {
            if (RelativeOptions != null)
            {
                var indexText = RelativeOptions.RelativeIndex switch
                {
                    DayOfWeekIndex.First => "1st",
                    DayOfWeekIndex.Second => "2nd", 
                    DayOfWeekIndex.Third => "3rd",
                    DayOfWeekIndex.Fourth => "4th",
                    DayOfWeekIndex.Last => "last",
                    _ => RelativeOptions.RelativeIndex.ToString()
                };
                
                var dayName = GetDayName(RelativeOptions.RelativeDayOfWeek);
                return $" on the {indexText} {dayName}";
            }
            
            var dayOfMonth = DayOfMonth > 0 ? DayOfMonth : startDate.Day;
            var suffix = GetOrdinalSuffix(dayOfMonth);
            return $" on the {dayOfMonth}{suffix}";
        }
    }
}