using NodaTime;

namespace Scheduler.Core.Contracts
{
    public interface IFrequency
    {
        LocalDate? ExpirationDate { get; }
        bool IsExpired(LocalDate currentDate);
        string GenerateDescription(LocalDate startDate, LocalTime startTime, LocalTime endTime, DateTimeZone timeZone);
    }
}