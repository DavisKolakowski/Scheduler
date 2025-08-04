using NodaTime;
using Scheduler.Core.Contracts;

namespace Scheduler.Core.Frequencies
{
    public class OneTime : IFrequency
    {
        private LocalDate? _calculatedExpirationDate;
        
        public LocalDate? ExpirationDate => _calculatedExpirationDate;

        public bool IsExpired(LocalDate currentDate)
        {
            return ExpirationDate.HasValue && currentDate > ExpirationDate.Value;
        }

        internal void SetExpirationDate(LocalDate startDate, LocalTime startTime, LocalTime endTime)
        {
            if (endTime < startTime)
            {
                _calculatedExpirationDate = startDate.PlusDays(1);
            }
            else
            {
                _calculatedExpirationDate = startDate;
            }
        }

        public string GenerateDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone)
        {
            var startTimeFormatted = startTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var endTimeFormatted = endTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            var dateFormatted = startDate.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
            
            var description = $"Occurs once on {dateFormatted} at {startTimeFormatted}-{endTimeFormatted} ({timeZone.Id})";
            
            if (endTime < startTime)
            {
                var endDateFormatted = startDate.PlusDays(1).ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
                description = $"Occurs once starting on {dateFormatted} at {startTimeFormatted} and ending on {endDateFormatted} at {endTimeFormatted} ({timeZone.Id})";
            }
            
            return description;
        }
    }
}