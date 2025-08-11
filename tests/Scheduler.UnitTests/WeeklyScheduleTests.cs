using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class WeeklyScheduleTests : BaseScheduleTests
{
    [Fact]
    public void WeeklySchedule_BasicWeekly_ShouldReturnCorrectOccurrences()
    {
        // Arrange - Start on Tuesday Aug 5, 2025
        var clock = CreateClock(2025, 8, 5, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 5), // Tuesday
            TestTime(18, 0),
            TestTime(19, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Weekly().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal("Weekly", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Equal(4, upcoming.Count);
        
        // Should be on consecutive Tuesdays
        Assert.Equal(IsoDayOfWeek.Tuesday, upcoming[0].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Tuesday, upcoming[1].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Tuesday, upcoming[2].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Tuesday, upcoming[3].DayOfWeek);
        
        // Verify 7-day intervals
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date);
            Assert.Equal(7, diff.Days);
        }
    }

    [Fact]
    public void WeeklySchedule_MultipleDaysOfWeek_ShouldReturnCorrectOccurrences()
    {
        // Arrange - Start on Monday Aug 4, 2025
        var clock = CreateClock(2025, 8, 4, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 4), // Monday
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 }))) // Mon, Wed, Fri
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // Should alternate Mon, Wed, Fri pattern
        Assert.Equal(IsoDayOfWeek.Monday, upcoming[0].DayOfWeek);    // Aug 4
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[1].DayOfWeek); // Aug 6
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[2].DayOfWeek);    // Aug 8
        Assert.Equal(IsoDayOfWeek.Monday, upcoming[3].DayOfWeek);    // Aug 11
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[4].DayOfWeek); // Aug 13
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[5].DayOfWeek);    // Aug 15
    }

    [Fact]
    public void WeeklySchedule_BiWeekly_ShouldSkipAlternateWeeks()
    {
        // Arrange - Start on Sunday Aug 3, 2025
        var clock = CreateClock(2025, 8, 3, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 3), // Sunday
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o =>
            {
                o.Interval = 2; // Every 2 weeks
                o.UseDaysOfWeek(list => list.AddRange(new[] { 7, 1 })); // Sun, Mon
            }).Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // Should be Sun Aug 3, Mon Aug 4, Sun Aug 17, Mon Aug 18, Sun Aug 31, Mon Sep 1
        Assert.Equal(3, upcoming[0].Day);  // Aug 3 (Sun)
        Assert.Equal(4, upcoming[1].Day);  // Aug 4 (Mon)
        Assert.Equal(17, upcoming[2].Day); // Aug 17 (Sun) - 2 weeks later
        Assert.Equal(18, upcoming[3].Day); // Aug 18 (Mon)
        Assert.Equal(31, upcoming[4].Day); // Aug 31 (Sun) - 2 weeks later
        Assert.Equal(1, upcoming[5].Day);  // Sep 1 (Mon)
        Assert.Equal(9, upcoming[5].Month); // September
    }

    [Fact]
    public void WeeklySchedule_StartInMiddleOfWeek_ShouldHandleCorrectly()
    {
        // Arrange - Start on Wednesday, but schedule for Mon/Wed/Fri
        var clock = CreateClock(2025, 8, 6, 8, 0); // Wednesday Aug 6
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 6), // Wednesday
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 }))) // Mon, Wed, Fri
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        // Should start with Wed Aug 6, then Fri Aug 8, then Mon Aug 11, etc.
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[0].DayOfWeek); // Aug 6
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[1].DayOfWeek);    // Aug 8
        Assert.Equal(IsoDayOfWeek.Monday, upcoming[2].DayOfWeek);    // Aug 11
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[3].DayOfWeek); // Aug 13
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[4].DayOfWeek);    // Aug 15
    }

    [Fact]
    public void WeeklySchedule_AllDaysOfWeek_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 4, 8, 0); // Monday
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(9, 0),
            TestTime(10, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7 }))) // All days
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(7).ToList();

        // Assert
        Assert.Equal(7, upcoming.Count);
        
        // Should be every day of the week
        for (int i = 0; i < 7; i++)
        {
            Assert.Equal((IsoDayOfWeek)(i + 1), upcoming[i].DayOfWeek);
        }
    }

    [Fact]
    public void WeeklySchedule_InvalidDaysOfWeek_ShouldIgnore()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 4, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(9, 0),
            TestTime(10, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 0, 1, 8, 3, -1, 5 }))) // Invalid days mixed with valid
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        // Should only include valid days (1, 3, 5 = Mon, Wed, Fri)
        Assert.Equal(IsoDayOfWeek.Monday, upcoming[0].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Wednesday, upcoming[1].DayOfWeek);
        Assert.Equal(IsoDayOfWeek.Friday, upcoming[2].DayOfWeek);
    }

    [Fact]
    public void WeeklySchedule_SingleDaySpecified_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 6, 8, 0); // Wednesday
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 6),
            TestTime(14, 0),
            TestTime(15, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.Add(3))) // Only Wednesday
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);
        
        // All should be Wednesdays
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Wednesday, occurrence.DayOfWeek);
        }
    }

    [Fact]
    public void WeeklySchedule_AcrossMonthBoundary_ShouldWork()
    {
        // Arrange - Late July going into August
        var clock = CreateClock(2025, 7, 28, 8, 0); // Monday July 28
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 7, 28),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 5 }))) // Mon, Fri
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // Should cross month boundary correctly
        Assert.Equal(7, upcoming[0].Month);  // July 28 (Mon)
        Assert.Equal(8, upcoming[1].Month);  // August 1 (Fri)
        Assert.Equal(8, upcoming[2].Month);  // August 4 (Mon)
        Assert.Equal(8, upcoming[3].Month);  // August 8 (Fri)
    }

    [Fact]
    public void WeeklySchedule_WithEndDate_ShouldStopCorrectly()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 4, 8, 0); // Monday
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring(TestDate(2025, 8, 15)) // End on Aug 15
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 }))) // Mon, Wed, Fri
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(20).ToList();

        // Assert
        // Should stop at Aug 15 (Friday)
        Assert.True(upcoming.Count <= 6); // Max possible in this range
        Assert.True(upcoming.All(o => o.Date <= TestDate(2025, 8, 15)));
    }

    [Fact]
    public void WeeklySchedule_DuringActiveOccurrence_ShouldReturnCurrent()
    {
        // Arrange - Set clock during a Wednesday occurrence
        var clock = CreateClock(2025, 8, 6, 12, 30); // Wed Aug 6, 12:30 PM
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 6),
            TestTime(12, 0),
            TestTime(13, 0), // 12-1 PM
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.Add(3))) // Wednesday only
            .Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal(6, nextOccurrence.Value.Day); // Should be today (currently active)
    }

    [Theory]
    [InlineData(1)] // Monday
    [InlineData(2)] // Tuesday  
    [InlineData(3)] // Wednesday
    [InlineData(4)] // Thursday
    [InlineData(5)] // Friday
    [InlineData(6)] // Saturday
    [InlineData(7)] // Sunday
    public void WeeklySchedule_EachDayOfWeek_ShouldWork(int dayOfWeek)
    {
        // Arrange
        var clock = CreateClock(2025, 8, 4, 8, 0); // Monday Aug 4
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 4),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.Add(dayOfWeek)))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        foreach (var occurrence in upcoming)
        {
            Assert.Equal((IsoDayOfWeek)dayOfWeek, occurrence.DayOfWeek);
        }
    }

    [Fact]
    public void WeeklySchedule_Description_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 5, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 5), // Tuesday
            TestTime(18, 0),
            TestTime(19, 0),
            TestTimeZone);

        var weeklySchedule = builder.Recurring().Weekly().Build();
        var multiDaySchedule = builder.Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 })))
            .Build();

        // Act
        var weeklyDescription = weeklySchedule.Description;
        var multiDayDescription = multiDaySchedule.Description;

        // Assert
        Assert.Contains("weekly", weeklyDescription);
        Assert.Contains("on Tuesday", weeklyDescription);
        Assert.Contains("on Monday, Wednesday, and Friday", multiDayDescription);
        Assert.Contains("6:00 PM - 7:00 PM", weeklyDescription);
    }
}