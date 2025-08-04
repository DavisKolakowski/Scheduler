using Scheduler.Core.Frequencies.Base;
using NodaTime;

namespace Scheduler.Core.Frequencies
{
    public class OneTime : Frequency
    {
        public override string GetScheduleDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            var startTimeFormatted = startTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var endTimeFormatted = endTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var dateFormatted = startDate.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
            
            return $"Occurs once on {dateFormatted} at {startTimeFormatted}-{endTimeFormatted} ({timeZone.Id})";
        }
    }
}