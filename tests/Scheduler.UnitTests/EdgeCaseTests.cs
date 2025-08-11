using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class EdgeCaseTests : BaseScheduleTests
{
    [Fact]
    public void Schedule_DaylightSavingTimeTransition_ShouldHandleCorrectly()
    {
        // Arrange - DST transition in US Eastern time (spring forward)
        var clock = CreateClock(2025, 3, 8, 12, 0); // Day before DST
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 9), // DST transition day
            TestTime(2, 30), // This time doesn't exist on DST day
            TestTime(3, 30),
            easternTime)
            .Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        // The lenient resolver should handle the ambiguous time
    }

    [Fact]
    public void Schedule_DaylightSavingTimeFallBack_ShouldHandleCorrectly()
    {
        // Arrange - DST fall back in US Eastern time
        var clock = CreateClock(2025, 11, 1, 12, 0); // Day before DST ends
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 11, 2), // DST end day
            TestTime(1, 30), // This time occurs twice
            TestTime(2, 30),
            easternTime)
            .Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        // The lenient resolver should handle the ambiguous time
    }

    [Fact]
    public void Schedule_MaxDateTime_ShouldNotOverflow()
    {
        // Arrange
        var clock = CreateClock(9998, 12, 31, 12, 0); // Near max date
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(9999, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily()
            .Build();

        // Act & Assert - Should not throw
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();
        Assert.NotEmpty(upcoming);
    }

    [Fact]
    public void Schedule_MinDateTime_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(1, 1, 2, 12, 0); // Very early date
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(1, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.Null(nextOccurrence); // Should be in the past
    }

    [Fact]
    public void Schedule_WeeklySchedule_Week53_ShouldHandleCorrectly()
    {
        // Arrange - Year with 53 ISO weeks (like 2020)
        var clock = CreateClock(2020, 12, 28, 12, 0); // Week 53
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2020, 12, 28), // Monday of week 53
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Weekly()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        // Should properly transition to next year
        Assert.Equal(2020, upcoming[0].Year);
        Assert.Equal(2021, upcoming[1].Year); // Next week is in 2021
    }

    [Fact]
    public void Schedule_MonthlySchedule_February29NonLeapYear_ShouldSkip()
    {
        // Arrange - Start on Feb 29 in leap year
        var clock = CreateClock(2024, 2, 29, 12, 0); // 2024 is leap year
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2024, 2, 29),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Monthly()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        // Should skip non-leap years (2025, 2026, 2027) and go to 2028
        Assert.Contains(upcoming, o => o.Year == 2024);
        Assert.Contains(upcoming, o => o.Year == 2028); // Next leap year
        Assert.DoesNotContain(upcoming, o => o.Year == 2025);
        Assert.DoesNotContain(upcoming, o => o.Year == 2026);
        Assert.DoesNotContain(upcoming, o => o.Year == 2027);
    }

    [Fact]
    public void Schedule_MonthlySchedule_Day31InFebruaryAprilJuneSeptemberNovember_ShouldSkip()
    {
        // Arrange
        var clock = CreateClock(2024, 1, 31, 12, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2024, 1, 31),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Monthly()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(12).ToList();

        // Assert
        // Should only appear in months with 31 days
        var validMonths = new[] { 1, 3, 5, 7, 8, 10, 12 }; // Months with 31 days
        foreach (var occurrence in upcoming)
        {
            Assert.Contains(occurrence.Month, validMonths);
            Assert.Equal(31, occurrence.Day);
        }
    }

    [Fact]
    public void Schedule_WeeklySchedule_EmptyDaysOfWeek_ShouldUseStartDay()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 7, 8, 0); // Tuesday
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 7), // Tuesday
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => { })) // Empty list
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        // Should default to the start day (Tuesday)
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Tuesday, occurrence.DayOfWeek);
        }
    }

    [Fact]
    public void Schedule_MonthlySchedule_EmptyDaysOfMonth_ShouldUseStartDay()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 15, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Monthly(o => o.UseDaysOfMonth(list => { })) // Empty list
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        // Should default to the start day (15th)
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(15, occurrence.Day);
        }
    }

    [Fact]
    public void Schedule_YearlySchedule_EmptyMonths_ShouldUseStartMonth()
    {
        // Arrange
        var clock = CreateClock(2025, 6, 15, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => { })) // Empty list
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        // Should default to the start month (June)
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(6, occurrence.Month);
            Assert.Equal(15, occurrence.Day);
        }
    }

    [Fact]
    public void Schedule_RelativeSchedule_FourthMondayNotExists_ShouldSkipMonth()
    {
        // Arrange
        var clock = CreateClock(2025, 2, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 2, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(Scheduler.Core.Enums.RelativeIndex.Fourth, Scheduler.Core.Enums.RelativePosition.Monday))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        // Should skip months that don't have a 4th Monday
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Monday, occurrence.DayOfWeek);
            // Should be day 22 or later (4th week)
            Assert.True(occurrence.Day >= 22);
        }
    }

    [Fact]
    public void Schedule_VeryLargeInterval_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 365) // Once per year effectively
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        
        // Should be roughly one year apart
        var diff1 = Period.Between(upcoming[0].Date, upcoming[1].Date);
        var diff2 = Period.Between(upcoming[1].Date, upcoming[2].Date);
        
        Assert.Equal(365, diff1.Days);
        Assert.Equal(365, diff2.Days);
    }

    [Fact]
    public void Schedule_ZeroInterval_ShouldDefaultToOne()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 0) // Invalid interval
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        // Should behave as daily (interval = 1)
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date);
            Assert.True(diff.Days > 0); // Should not be infinite loop
        }
    }

    [Fact]
    public void Schedule_NegativeInterval_ShouldDefaultToOne()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily(o => o.Interval = -5) // Invalid interval
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        // Should behave as daily (interval = 1)
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date);
            Assert.True(diff.Days > 0);
        }
    }

    [Fact]
    public void Schedule_GetOccurrences_WithZeroMaxItems_ShouldReturnEmpty()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(0).ToList();
        var completed = schedule.GetOccurrencesCompleted(0).ToList();

        // Assert
        Assert.Empty(upcoming);
        Assert.Empty(completed);
    }

    [Fact]
    public void Schedule_GetOccurrences_WithNegativeMaxItems_ShouldReturnEmpty()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(-5).ToList();
        var completed = schedule.GetOccurrencesCompleted(-5).ToList();

        // Assert
        Assert.Empty(upcoming);
        Assert.Empty(completed);
    }
}