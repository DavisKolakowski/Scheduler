using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class DailyScheduleTests : BaseScheduleTests
{
    [Fact]
    public void DailySchedule_BasicDaily_ShouldReturnCorrectOccurrences()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        Assert.Equal("Daily", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Equal(5, upcoming.Count);
        
        // Verify occurrences are on consecutive days
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date);
            Assert.Equal(1, diff.Days);
        }
    }

    [Fact]
    public void DailySchedule_WithInterval_ShouldSkipCorrectDays()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily(o => o.Interval = 3).Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        
        // Should be on Aug 1, Aug 4, Aug 7
        Assert.Equal(1, upcoming[0].Day);
        Assert.Equal(4, upcoming[1].Day);
        Assert.Equal(7, upcoming[2].Day);
    }

    [Fact]
    public void DailySchedule_WithEndDate_ShouldStopAtEndDate()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring(TestDate(2025, 8, 5)).Daily().Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        // Assert
        Assert.Equal(5, upcoming.Count); // Aug 1, 2, 3, 4, 5
        Assert.Equal(5, upcoming.Last().Day);
    }

    [Fact]
    public void DailySchedule_StartDateInPast_ShouldCalculateCorrectNext()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 10, 8, 0); // Start 9 days after the schedule start
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var completed = schedule.GetOccurrencesCompleted(5).ToList();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal(10, nextOccurrence.Value.Day); // Should be today (Aug 10)
        Assert.Equal(5, completed.Count); // Should have 5 completed occurrences
    }

    [Fact]
    public void DailySchedule_WithIntervalStartInPast_ShouldCalculateCorrectNext()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 10, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1), // Start Aug 1
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily(o => o.Interval = 3).Build(); // Every 3 days

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        // Aug 1, 4, 7, 10, 13... so next should be Aug 10
        Assert.Equal(10, nextOccurrence.Value.Day);
    }

    [Fact]
    public void DailySchedule_DuringActiveOccurrence_ShouldReturnCurrentAsNext()
    {
        // Arrange - Clock set during the daily occurrence
        var clock = CreateClock(2025, 8, 1, 12, 0); // Noon on Aug 1
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0), // 9 AM to 5 PM
            TestTimeZone);

        var schedule = builder.Recurring().Daily().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal(1, nextOccurrence.Value.Day); // Should be today (currently active)
    }

    [Fact]
    public void DailySchedule_AfterDailyOccurrence_ShouldReturnTomorrow()
    {
        // Arrange - Clock set after today's occurrence
        var clock = CreateClock(2025, 8, 1, 18, 0); // 6 PM on Aug 1 (after 5 PM end)
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var previousOccurrence = schedule.GetPreviousOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.NotNull(previousOccurrence);
        Assert.Equal(2, nextOccurrence.Value.Day); // Tomorrow (Aug 2)
        Assert.Equal(1, previousOccurrence.Value.Day); // Today (Aug 1)
    }

    [Fact]
    public void DailySchedule_AcrossMonthBoundary_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 7, 30, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 7, 30),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily().Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        Assert.Equal(5, upcoming.Count);
        Assert.Equal(7, upcoming[0].Month); // July 30
        Assert.Equal(7, upcoming[1].Month); // July 31
        Assert.Equal(8, upcoming[2].Month); // August 1
        Assert.Equal(8, upcoming[3].Month); // August 2
        Assert.Equal(8, upcoming[4].Month); // August 3
    }

    [Fact]
    public void DailySchedule_AcrossYearBoundary_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2024, 12, 30, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 12, 30),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily().Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        Assert.Equal(5, upcoming.Count);
        Assert.Equal(2024, upcoming[0].Year); // Dec 30, 2024
        Assert.Equal(2024, upcoming[1].Year); // Dec 31, 2024
        Assert.Equal(2025, upcoming[2].Year); // Jan 1, 2025
        Assert.Equal(2025, upcoming[3].Year); // Jan 2, 2025
        Assert.Equal(2025, upcoming[4].Year); // Jan 3, 2025
    }

    [Fact]
    public void DailySchedule_LeapYear_ShouldHandleFebruary29()
    {
        // Arrange
        var clock = CreateClock(2024, 2, 28, 8, 0); // 2024 is a leap year
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 2, 28),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily().Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        Assert.Equal(28, upcoming[0].Day); // Feb 28
        Assert.Equal(29, upcoming[1].Day); // Feb 29 (leap day)
        Assert.Equal(1, upcoming[2].Day);  // Mar 1
        Assert.Equal(3, upcoming[2].Month);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(30)]
    public void DailySchedule_VariousIntervals_ShouldWork(int interval)
    {
        // Arrange
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Daily(o => o.Interval = interval).Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        
        // Verify correct interval between occurrences
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date);
            Assert.Equal(interval, diff.Days);
        }
    }

    [Fact]
    public void DailySchedule_Description_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var dailySchedule = builder.Recurring().Daily().Build();
        var intervalSchedule = builder.Recurring().Daily(o => o.Interval = 3).Build();

        // Act
        var dailyDescription = dailySchedule.Description;
        var intervalDescription = intervalSchedule.Description;

        // Assert
        Assert.Contains("daily", dailyDescription);
        Assert.Contains("every 3 days", intervalDescription);
        Assert.Contains("9:00 AM - 5:00 PM", dailyDescription);
        Assert.Contains("August 1st, 2025", dailyDescription);
    }
}