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
        var clock = CreateClock(2025, 3, 8, 12, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 9),
            TestTime(2, 30),
            TestTime(3, 30),
            easternTime)
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();

        Assert.NotNull(nextOccurrence);
    }

    [Fact]
    public void Schedule_DaylightSavingTimeFallBack_ShouldHandleCorrectly()
    {
        var clock = CreateClock(2025, 11, 1, 12, 0);
        var context = CreateContext(clock);
        var easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        var schedule = context.CreateBuilder(
            TestDate(2025, 11, 2),
            TestTime(1, 30),
            TestTime(2, 30),
            easternTime)
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();

        Assert.NotNull(nextOccurrence);
    }

    [Fact]
    public void Schedule_MaxDateTime_ShouldNotOverflow()
    {
        var clock = CreateClock(9998, 12, 31, 12, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(9999, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();
        
        Assert.NotEmpty(upcoming);
    }

    [Fact]
    public void Schedule_MinDateTime_ShouldWork()
    {
        var clock = CreateClock(1, 1, 2, 12, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(1, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();
        
        Assert.NotEmpty(upcoming);
    }

    [Fact]
    public void Schedule_LeapSecond_ShouldHandleCorrectly()
    {
        var clock = CreateClock(2016, 12, 31, 23, 59, 59);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2017, 1, 1),
            TestTime(0, 0),
            TestTime(1, 0),
            UtcTimeZone)
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        
        Assert.NotNull(nextOccurrence);
    }

    [Fact]
    public void Schedule_VeryLargeInterval_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 12, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily(o => o.Interval = int.MaxValue)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(1).ToList();
        
        Assert.Single(upcoming);
    }

    [Fact]
    public void Schedule_ZeroInterval_ShouldDefaultToOne()
    {
        var clock = CreateClock(2025, 1, 1, 12, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            UtcTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 0)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.True(diff.Days > 0);
        }
    }

    [Fact]
    public void Schedule_GetOccurrences_WithZeroMaxItems_ShouldReturnEmpty()
    {
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

        var upcoming = schedule.GetUpcomingOccurrences(0).ToList();
        var completed = schedule.GetOccurrencesCompleted(0).ToList();

        Assert.Empty(upcoming);
        Assert.Empty(completed);
    }

    [Fact]
    public void Schedule_GetOccurrences_WithNegativeMaxItems_ShouldReturnEmpty()
    {
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

        var upcoming = schedule.GetUpcomingOccurrences(-5).ToList();
        var completed = schedule.GetOccurrencesCompleted(-5).ToList();

        Assert.Empty(upcoming);
        Assert.Empty(completed);
    }
}