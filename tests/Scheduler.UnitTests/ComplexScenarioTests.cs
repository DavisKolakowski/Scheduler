using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Enums;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class ComplexScenarioTests : BaseScheduleTests
{
    [Fact]
    public void ComplexScenario_QuarterlyBusinessMeeting_ShouldWork()
    {
        // Arrange - First Friday of January, April, July, October
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 3), // First Friday of January 2025
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 4, 7, 10 })); // Quarterly
                o.UseRelative(RelativeIndex.First, RelativePosition.Friday);
            })
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(8).ToList(); // 2 years worth

        // Assert
        Assert.Equal(8, upcoming.Count);

        // Verify quarterly pattern
        var months = upcoming.Select(o => o.Month).ToList();
        var expectedMonths = new[] { 1, 4, 7, 10, 1, 4, 7, 10 };
        Assert.Equal(expectedMonths, months);

        // All should be Fridays
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Friday, o.DayOfWeek));

        // Should be first Friday of each month
        Assert.All(upcoming, o => Assert.True(o.Day <= 7));
    }

    [Fact]
    public void ComplexScenario_BiWeeklyPayroll_ShouldWork()
    {
        // Arrange - Every other Friday
        var clock = CreateClock(2025, 1, 3, 8, 0); // Friday Jan 3
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 3), // Friday
            TestTime(12, 0),
            TestTime(12, 30),
            TestTimeZone)
            .Recurring()
            .Weekly(o =>
            {
                o.Interval = 2; // Every 2 weeks
                o.UseDaysOfWeek(list => list.Add(5)); // Friday
            })
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(8).ToList();

        // Assert
        Assert.Equal(8, upcoming.Count);

        // All should be Fridays
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Friday, o.DayOfWeek));

        // Should be every 2 weeks
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.Equal(14, diff.Days); // 2 weeks = 14 days
        }
    }

    [Fact]
    public void ComplexScenario_MonthlyLastWorkingDay_ShouldWork()
    {
        // Arrange - Last weekday of each month
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 31), // Last day of January (Friday)
            TestTime(17, 0),
            TestTime(17, 30),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.Weekday))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(12).ToList(); // One year

        // Assert
        Assert.Equal(12, upcoming.Count);

        // All should be weekdays
        Assert.All(upcoming, o => 
            Assert.True(o.DayOfWeek >= IsoDayOfWeek.Monday && o.DayOfWeek <= IsoDayOfWeek.Friday));

        // Should be in the last week of each month
        Assert.All(upcoming, o => Assert.True(o.Day >= 25));

        // Each occurrence should be in a different month
        var months = upcoming.Select(o => o.Month).ToList();
        Assert.Equal(Enumerable.Range(1, 12), months);
    }

    [Fact]
    public void ComplexScenario_SchoolSemesterSchedule_ShouldWork()
    {
        // Arrange - Class every Tuesday and Thursday from Jan 15 to May 15
        var clock = CreateClock(2025, 1, 14, 8, 0); // Day before start
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 16), // Thursday Jan 16
            TestTime(10, 0),
            TestTime(11, 30),
            TestTimeZone)
            .Recurring(TestDate(2025, 5, 15)) // End date
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 2, 4 }))) // Tue, Thu
            .Build();

        // Act
        var allOccurrences = schedule.GetUpcomingOccurrences(100).ToList();

        // Assert
        Assert.True(allOccurrences.Count > 0);
        Assert.True(allOccurrences.Count < 100); // Should end before hitting limit

        // All should be Tuesday or Thursday
        Assert.All(allOccurrences, o => 
            Assert.True(o.DayOfWeek == IsoDayOfWeek.Tuesday || o.DayOfWeek == IsoDayOfWeek.Thursday));

        // Should not exceed end date
        Assert.All(allOccurrences, o => Assert.True(o.Date <= TestDate(2025, 5, 15)));

        // Should be within the semester timeframe
        Assert.All(allOccurrences, o => 
            Assert.True(o.Date >= TestDate(2025, 1, 16) && o.Date <= TestDate(2025, 5, 15)));
    }

    [Fact]
    public void ComplexScenario_MaintenanceWindow_ShouldWork()
    {
        // Arrange - First Sunday of each month at 2 AM (overnight maintenance)
        var clock = CreateClock(2025, 1, 5, 12, 0); // First Sunday of January
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 5), // First Sunday
            TestTime(2, 0),
            TestTime(6, 0), // 4-hour maintenance window
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Sunday))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(12).ToList();

        // Assert
        Assert.Equal(12, upcoming.Count);

        // All should be Sundays
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Sunday, o.DayOfWeek));

        // All should be at 2 AM
        Assert.All(upcoming, o => Assert.Equal(2, o.Hour));

        // Should be first Sunday of each month (day 1-7)
        Assert.All(upcoming, o => Assert.True(o.Day <= 7));

        // Verify duration is 4 hours
        Assert.All(upcoming, o => 
            Assert.Equal("04:00", schedule.OccurrenceDuration));
    }

    [Fact]
    public void ComplexScenario_ConferenceCall_MultipleTimeZones_ShouldWork()
    {
        // Arrange - Weekly call at 3 PM Eastern (which is 8 PM London, 9 AM Pacific next day)
        var clock = CreateClock(2025, 1, 6, 12, 0); // Monday
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6), // Monday
            TestTime(15, 0), // 3 PM Eastern
            TestTime(16, 0),
            easternTime)
            .Recurring()
            .Weekly()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);

        // All should be Mondays at 3 PM Eastern
        Assert.All(upcoming, o => 
        {
            Assert.Equal(IsoDayOfWeek.Monday, o.DayOfWeek);
            Assert.Equal(15, o.Hour);
            Assert.Equal(easternTime, o.Zone);
        });

        // Verify the UTC times are consistent
        var utcTimes = upcoming.Select(o => o.ToInstant()).ToList();
        for (int i = 0; i < utcTimes.Count - 1; i++)
        {
            var diff = utcTimes[i + 1] - utcTimes[i];
            Assert.Equal(Duration.FromDays(7), diff); // Exactly 1 week apart
        }
    }

    [Fact]
    public void ComplexScenario_LeapYearHandling_ShouldWork()
    {
        // Arrange - February 29th yearly schedule (should only occur in leap years)
        var clock = CreateClock(2024, 2, 29, 8, 0); // Leap day 2024
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2024, 2, 29), // Leap day
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Recurring()
            .Yearly()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        // Assert
        // Should only include leap years
        Assert.All(upcoming, o => 
        {
            Assert.Equal(2, o.Month); // February
            Assert.Equal(29, o.Day);  // 29th
            
            // Verify it's a leap year
            var year = o.Year;
            var isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
            Assert.True(isLeapYear);
        });

        // Should include 2024, 2028, 2032, etc.
        var years = upcoming.Select(o => o.Year).ToList();
        var expectedYears = new[] { 2024, 2028, 2032, 2036, 2040, 2044, 2048, 2052, 2056, 2060 };
        Assert.Equal(expectedYears.Take(upcoming.Count), years);
    }

    [Fact]
    public void ComplexScenario_HolidaySchedule_ShouldWork()
    {
        // Arrange - Thanksgiving (4th Thursday in November) every year
        var clock = CreateClock(2024, 11, 28, 8, 0); // Thanksgiving 2024
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2024, 11, 28), // Thanksgiving 2024 (4th Thursday)
            TestTime(12, 0),
            TestTime(15, 0), // Thanksgiving dinner time
            TestTimeZone)
            .Recurring()
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.Add(11)); // November
                o.UseRelative(RelativeIndex.Fourth, RelativePosition.Thursday);
            })
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        Assert.Equal(5, upcoming.Count);

        // All should be in November
        Assert.All(upcoming, o => Assert.Equal(11, o.Month));

        // All should be Thursdays
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Thursday, o.DayOfWeek));

        // Should be 4th Thursday (typically day 22-28)
        Assert.All(upcoming, o => Assert.True(o.Day >= 22 && o.Day <= 28));

        // Verify specific years and dates
        Assert.Equal(2024, upcoming[0].Year);
        Assert.Equal(2025, upcoming[1].Year);
        Assert.Equal(2026, upcoming[2].Year);
    }

    [Fact]
    public void ComplexScenario_EndOfMonthBilling_ShouldWork()
    {
        // Arrange - Last day of each month
        var clock = CreateClock(2025, 1, 31, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 31),
            TestTime(23, 59), // End of day
            TestTime(23, 59),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.Day))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(12).ToList(); // Full year

        // Assert
        Assert.Equal(12, upcoming.Count);

        // Verify last days of each month for 2025 (non-leap year)
        var expectedDays = new[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        
        for (int i = 0; i < 12; i++)
        {
            Assert.Equal(i + 1, upcoming[i].Month); // Month 1-12
            Assert.Equal(expectedDays[i], upcoming[i].Day); // Last day of month
            Assert.Equal(23, upcoming[i].Hour); // 11 PM
            Assert.Equal(59, upcoming[i].Minute); // 59 minutes
        }
    }

    [Fact]
    public void ComplexScenario_WorkshiftRotation_ShouldWork()
    {
        // Arrange - Every 3rd day starting Monday (rotating shift)
        var clock = CreateClock(2025, 1, 6, 8, 0); // Monday
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6), // Monday
            TestTime(7, 0),
            TestTime(19, 0), // 12-hour shift
            TestTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 3) // Every 3 days
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        // Assert
        Assert.Equal(10, upcoming.Count);

        // Verify 3-day intervals
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.Equal(3, diff.Days);
        }

        // Verify rotating days of week: Mon, Thu, Sun, Wed, Sat, Tue, Fri, Mon, ...
        var daysOfWeek = upcoming.Select(o => o.DayOfWeek).ToList();
        Assert.Equal(IsoDayOfWeek.Monday, daysOfWeek[0]);
        Assert.Equal(IsoDayOfWeek.Thursday, daysOfWeek[1]);
        Assert.Equal(IsoDayOfWeek.Sunday, daysOfWeek[2]);
        Assert.Equal(IsoDayOfWeek.Wednesday, daysOfWeek[3]);

        // All should be 12-hour shifts
        Assert.All(upcoming, o => 
        {
            Assert.Equal(7, o.Hour); // 7 AM start
            Assert.Equal("12:00", schedule.OccurrenceDuration); // 12 hours
        });
    }
}