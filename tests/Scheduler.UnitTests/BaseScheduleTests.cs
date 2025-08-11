using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Models;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

/// <summary>
/// Base class for all schedule tests providing common test infrastructure
/// </summary>
public abstract class BaseScheduleTests
{
    protected readonly DateTimeZone TestTimeZone = DateTimeZoneProviders.Tzdb["America/New_York"];
    protected readonly DateTimeZone UtcTimeZone = DateTimeZone.Utc;
    protected readonly CalendarSystem TestCalendar = CalendarSystem.Iso;

    /// <summary>
    /// Creates a FakeClock for testing with a specific instant
    /// </summary>
    protected FakeClock CreateClock(Instant instant) => new(instant);

    /// <summary>
    /// Creates a FakeClock for testing with a specific date and time in UTC
    /// </summary>
    protected FakeClock CreateClock(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        var instant = Instant.FromUtc(year, month, day, hour, minute, second);
        return new FakeClock(instant);
    }

    /// <summary>
    /// Creates a ScheduleContext with a fake clock
    /// </summary>
    protected ScheduleContext CreateContext(IClock clock) => new(clock);

    /// <summary>
    /// Creates a test date
    /// </summary>
    protected LocalDate TestDate(int year, int month, int day) => new(year, month, day);

    /// <summary>
    /// Creates a test time
    /// </summary>
    protected LocalTime TestTime(int hour, int minute = 0) => new(hour, minute);

    /// <summary>
    /// Asserts that two ZonedDateTime values are equal
    /// </summary>
    protected void AssertZonedDateTimeEqual(ZonedDateTime? expected, ZonedDateTime? actual)
    {
        if (expected == null && actual == null) return;
        if (expected == null || actual == null)
        {
            Assert.True(false, $"Expected: {expected}, Actual: {actual}");
        }
        Assert.Equal(expected!.Value.ToInstant(), actual!.Value.ToInstant());
    }

    /// <summary>
    /// Asserts that a collection of ZonedDateTime values matches expected values
    /// </summary>
    protected void AssertOccurrences(IEnumerable<ZonedDateTime> expected, IEnumerable<ZonedDateTime> actual)
    {
        var expectedList = expected.ToList();
        var actualList = actual.ToList();
        
        Assert.Equal(expectedList.Count, actualList.Count);
        
        for (int i = 0; i < expectedList.Count; i++)
        {
            Assert.Equal(expectedList[i].ToInstant(), actualList[i].ToInstant());
        }
    }
}