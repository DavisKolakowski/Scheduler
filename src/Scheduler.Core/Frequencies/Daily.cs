using Scheduler.Core.Frequencies.Base;
using NodaTime;

namespace Scheduler.Core.Frequencies
{
    public class Daily : Recurring
    {
        public override string GetScheduleDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            var startTimeFormatted = startTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var endTimeFormatted = endTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var startDateFormatted = startDate.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
            
            var intervalText = Interval == 1 ? "every day" : $"every {Interval} days";
            var description = $"Occurs {intervalText} at {startTimeFormatted}-{endTimeFormatted} ({timeZone.Id}) starting on {startDateFormatted}";
            
            if (ExpirationDate.HasValue)
            {
                var expirationFormatted = ExpirationDate.Value.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
                description += $" until {expirationFormatted}";
            }
            
            return description;
        }
    }
}