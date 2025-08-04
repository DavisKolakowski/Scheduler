using Scheduler.Core.Frequencies.Base;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace Scheduler.Core.Frequencies
{
    public class Weekly : Recurring
    {
        public List<int> Days { get; set; } = new List<int>();
        
        public override string GetScheduleDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            var startTimeFormatted = startTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var endTimeFormatted = endTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var startDateFormatted = startDate.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
            
            var intervalText = Interval == 1 ? "every week" : $"every {Interval} weeks";
            var daysText = GetDaysText();
            
            var description = $"Occurs {intervalText}{daysText} at {startTimeFormatted}-{endTimeFormatted} ({timeZone.Id}) starting on {startDateFormatted}";
            
            if (ExpirationDate.HasValue)
            {
                var expirationFormatted = ExpirationDate.Value.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
                description += $" until {expirationFormatted}";
            }
            
            return description;
        }
        
        private string GetDaysText()
        {
            if (!Days.Any()) return "";
            
            if (Days.Count == 5 && Days.Contains(1) && Days.Contains(2) && Days.Contains(3) && Days.Contains(4) && Days.Contains(5))
            {
                return " on weekdays";
            }
            
            if (Days.Count == 2 && Days.Contains(6) && Days.Contains(7))
            {
                return " on weekends";
            }
            
            var dayNames = Days.Select(GetDayName).ToList();
            return $" on {string.Join(", ", dayNames)}";
        }
    }
}