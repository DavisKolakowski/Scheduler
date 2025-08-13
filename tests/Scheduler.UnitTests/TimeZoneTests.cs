using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Enums;

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
        var utcClock = CreateClock(2025, 6, 15, 6, 0);
        var context = CreateContext(utcClock);
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

        var schedule = context.CreateBuilder(
            TestDate(2025, 6, 16),
            TestTime(14, 0),
            TestTime(15, 0),
            timeZone)
            .Build();

        Assert.NotNull(schedule.NextOccurrence);
        Assert.Equal(timeZone, schedule.NextOccurrence.Value.Zone);
        Assert.Equal(14, schedule.NextOccurrence.Value.Hour);
    }

    [Fact]
    public void Schedule_SameMomentDifferentTimeZones_ShouldBeEqual()
    {
        var utcClock = CreateClock(2025, 6, 15, 16, 0);
        var context = CreateContext(utcClock);

        var utcSchedule = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(16, 0),
            TestTime(17, 0),
            DateTimeZone.Utc)
            .Build();

        var nySchedule = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(12, 0),
            TestTime(13, 0),
            DateTimeZoneProviders.Tzdb["America/New_York"])
            .Build();

        Assert.NotNull(utcSchedule.NextOccurrence);
        Assert.NotNull(nySchedule.NextOccurrence);
        Assert.Equal(utcSchedule.NextOccurrence.Value.ToInstant(), nySchedule.NextOccurrence.Value.ToInstant());
    }

    [Fact]
    public void Schedule_TimeZoneTransition_SpringForward_ShouldHandleGracefully()
    {
        var clock = CreateClock(2025, 3, 9, 6, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 9),
            TestTime(2, 30),
            TestTime(3, 30),
            easternTime)
            .Build();

        Assert.NotNull(schedule.NextOccurrence);
    }

    [Fact]
    public void Schedule_TimeZoneTransition_FallBack_ShouldHandleGracefully()
    {
        var clock = CreateClock(2025, 11, 2, 6, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 11, 2),
            TestTime(1, 30),
            TestTime(2, 30),
            easternTime)
            .Build();

        Assert.NotNull(schedule.NextOccurrence);
    }

    [Fact]
    public void Schedule_RecurringAcrossTimeZoneTransition_ShouldWork()
    {
        var clock = CreateClock(2025, 3, 8, 12, 0);
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

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(5, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(10, occurrence.Hour);
            Assert.Equal(easternTime, occurrence.Zone);
        }
    }

    [Fact]
    public void Schedule_WeeklyAcrossTimeZoneTransition_ShouldMaintainLocalTime()
    {
        var clock = CreateClock(2025, 3, 3, 12, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 3),
            TestTime(14, 0),
            TestTime(15, 0),
            easternTime)
            .Recurring()
            .Weekly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(14, occurrence.Hour);
            Assert.Equal(IsoDayOfWeek.Monday, occurrence.DayOfWeek);
        }
    }

    [Fact]
    public void Schedule_MonthlyAcrossTimeZoneTransition_ShouldMaintainLocalTime()
    {
        var clock = CreateClock(2025, 2, 15, 12, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 2, 15),
            TestTime(14, 0),
            TestTime(15, 0),
            easternTime)
            .Recurring()
            .Monthly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(14, occurrence.Hour);
            Assert.Equal(15, occurrence.Day);
        }
    }

    [Theory]
    [InlineData("Pacific/Kiritimati")]
    [InlineData("Pacific/Midway")]
    [InlineData("UTC")]
    public void Schedule_ExtremeTimeZones_ShouldWork(string timeZoneId)
    {
        var utcClock = CreateClock(2025, 6, 15, 0, 0);
        var context = CreateContext(utcClock);
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

        var schedule = context.CreateBuilder(
            TestDate(2025, 6, 16),
            TestTime(12, 0),
            TestTime(13, 0),
            timeZone)
            .Build();

        Assert.NotNull(schedule.NextOccurrence);
        Assert.Equal(timeZone, schedule.NextOccurrence.Value.Zone);
        Assert.Equal(12, schedule.NextOccurrence.Value.Hour);
    }

    [Fact]
    public void Schedule_ScheduleAtMidnight_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 6, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(0, 0),
            TestTime(1, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

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
        var clock = CreateClock(2025, 1, 1, 6, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(23, 59),
            TestTime(23, 59),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

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
        var clock = CreateClock(2025, 3, 8, 12, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 9),
            TestTime(1, 0),
            TestTime(4, 0),
            easternTime)
            .Build();

        Assert.NotNull(schedule.NextOccurrence);
        
        var duration = schedule.OccurrenceLength;
        Assert.NotNull(duration);
    }
}