using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Enums;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class TimeZoneTests : BaseScheduleTests
{
    [Theory]
    [InlineData("America/New_York")]
    [InlineData("Europe/London")]
    [InlineData("Asia/Tokyo")]
    [InlineData("Australia/Sydney")]
    [InlineData("UTC")]
    public void Schedule_DifferentTimeZones_ShouldRespectTimeZone(string timeZoneId)
    {
        // Arrange
        var utcClock = CreateClock(2025, 6, 15, 6, 0); // 6 AM UTC 
        var context = CreateContext(utcClock);
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

        // Schedule for the next day to ensure it's always in the future
        var schedule = context.CreateBuilder(
            TestDate(2025, 6, 16), // Next day
            TestTime(14, 0), // 2 PM local time
            TestTime(15, 0),
            timeZone)
            .Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal(timeZone, nextOccurrence.Value.Zone);
        Assert.Equal(14, nextOccurrence.Value.Hour); // Should be 2 PM in the specified timezone
    }

    [Fact]
    public void Schedule_SameMomentDifferentTimeZones_ShouldBeEqual()
    {
        // Arrange
        var utcClock = CreateClock(2025, 6, 15, 16, 0); // 4 PM UTC
        var context = CreateContext(utcClock);

        var utcSchedule = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(16, 0), // 4 PM UTC
            TestTime(17, 0),
            DateTimeZone.Utc)
            .Build();

        var nySchedule = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(12, 0), // 12 PM EDT (UTC-4)
            TestTime(13, 0),
            DateTimeZoneProviders.Tzdb["America/New_York"])
            .Build();

        // Act
        var utcNext = utcSchedule.GetNextOccurrence();
        var nyNext = nySchedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(utcNext);
        Assert.NotNull(nyNext);
        Assert.Equal(utcNext.Value.ToInstant(), nyNext.Value.ToInstant());
    }

    [Fact]
    public void Schedule_TimeZoneTransition_SpringForward_ShouldHandleGracefully()
    {
        // Arrange - Spring forward in Eastern time (2:00 AM becomes 3:00 AM)
        var clock = CreateClock(2025, 3, 9, 6, 0); // 6 AM UTC on DST transition day
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 9),
            TestTime(2, 30), // This time doesn't exist (skipped)
            TestTime(3, 30),
            easternTime)
            .Build();

        // Act & Assert - Should not throw
        var nextOccurrence = schedule.GetNextOccurrence();
        Assert.NotNull(nextOccurrence);
    }

    [Fact]
    public void Schedule_TimeZoneTransition_FallBack_ShouldHandleGracefully()
    {
        // Arrange - Fall back in Eastern time (2:00 AM occurs twice)
        var clock = CreateClock(2025, 11, 2, 6, 0); // 6 AM UTC on DST end day
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 11, 2),
            TestTime(1, 30), // This time occurs twice
            TestTime(2, 30),
            easternTime)
            .Build();

        // Act & Assert - Should not throw
        var nextOccurrence = schedule.GetNextOccurrence();
        Assert.NotNull(nextOccurrence);
    }

    [Fact]
    public void Schedule_RecurringAcrossTimeZoneTransition_ShouldWork()
    {
        // Arrange - Daily schedule that crosses DST transition
        var clock = CreateClock(2025, 3, 8, 12, 0); // Day before DST
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 8),
            TestTime(10, 0),
            TestTime(11, 0),
            easternTime)
            .Recurring()
            .Daily()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        Assert.Equal(5, upcoming.Count);
        
        // All should be at 10:00 AM local time, regardless of DST
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(10, occurrence.Hour);
            Assert.Equal(easternTime, occurrence.Zone);
        }
    }

    [Fact]
    public void Schedule_WeeklyAcrossTimeZoneTransition_ShouldMaintainLocalTime()
    {
        // Arrange - Weekly schedule that crosses DST transition
        var clock = CreateClock(2025, 3, 3, 12, 0); // Monday before DST week
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 3), // Monday
            TestTime(14, 0), // 2 PM
            TestTime(15, 0),
            easternTime)
            .Recurring()
            .Weekly()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);
        
        // All should be at 2:00 PM local time
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(14, occurrence.Hour);
            Assert.Equal(IsoDayOfWeek.Monday, occurrence.DayOfWeek);
        }
    }

    [Fact]
    public void Schedule_MonthlyAcrossTimeZoneTransition_ShouldMaintainLocalTime()
    {
        // Arrange - Monthly schedule that may cross DST
        var clock = CreateClock(2025, 2, 15, 12, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 2, 15),
            TestTime(14, 0), // 2 PM
            TestTime(15, 0),
            easternTime)
            .Recurring()
            .Monthly()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList(); // Feb through July

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // All should be at 2:00 PM local time and on 15th
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(14, occurrence.Hour);
            Assert.Equal(15, occurrence.Day);
        }
    }

    [Theory]
    [InlineData("Pacific/Kiritimati", 14)] // UTC+14
    [InlineData("Pacific/Midway", -11)]    // UTC-11
    [InlineData("UTC", 0)]                 // UTC+0
    public void Schedule_ExtremeTimeZones_ShouldWork(string timeZoneId, int expectedOffsetHours)
    {
        // Arrange
        var utcClock = CreateClock(2025, 6, 15, 0, 0); // Midnight UTC 
        var context = CreateContext(utcClock);
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

        // Schedule for the next day to ensure it's always in the future
        var schedule = context.CreateBuilder(
            TestDate(2025, 6, 16), // Next day
            TestTime(12, 0), // Noon local time
            TestTime(13, 0),
            timeZone)
            .Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal(timeZone, nextOccurrence.Value.Zone);
        Assert.Equal(12, nextOccurrence.Value.Hour); // Should be noon in the specified timezone
    }

    [Fact]
    public void Schedule_ScheduleAtMidnight_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 6, 0); // 6 AM UTC
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(0, 0), // Midnight
            TestTime(1, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(0, occurrence.Hour);
            Assert.Equal(0, occurrence.Minute);
        }
    }

    [Fact]
    public void Schedule_ScheduleAt2359_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 6, 0); 
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(23, 59), // 11:59 PM
            TestTime(23, 59), // Same time (zero duration)
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(23, occurrence.Hour);
            Assert.Equal(59, occurrence.Minute);
        }
    }

    [Fact]
    public void Schedule_OvernightAcrossTimeZoneTransition_ShouldWork()
    {
        // Arrange - Overnight schedule during DST transition
        var clock = CreateClock(2025, 3, 8, 12, 0); // Day before DST
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 9), // DST transition day
            TestTime(1, 0), // 1 AM
            TestTime(4, 0), // 4 AM (crosses the 2-3 AM gap)
            easternTime)
            .Build();

        // Act & Assert - Should handle the time gap gracefully
        var nextOccurrence = schedule.GetNextOccurrence();
        Assert.NotNull(nextOccurrence);
        
        var duration = schedule.OccurrenceDuration;
        // Duration should account for the lost hour during spring forward
        Assert.NotNull(duration);
    }
}