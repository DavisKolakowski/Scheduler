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
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal("Daily", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Equal(5, upcoming.Count);
        
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.Equal(1, diff.Days);
        }
    }

    [Fact]
    public void DailySchedule_WithInterval_ShouldSkipCorrectDays()
    {
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 3)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        Assert.Equal(1, upcoming[0].Day);
        Assert.Equal(4, upcoming[1].Day);
        Assert.Equal(7, upcoming[2].Day);
    }

    [Fact]
    public void DailySchedule_WithEndDate_ShouldStopAtEndDate()
    {
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring(TestDate(2025, 8, 5))
            .Daily()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        Assert.Equal(5, upcoming.Count);
        Assert.Equal(5, upcoming.Last().Day);
    }

    [Fact]
    public void DailySchedule_StartDateInPast_ShouldCalculateCorrectNext()
    {
        var clock = CreateClock(2025, 8, 10, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var completed = schedule.GetOccurrencesCompleted(5).ToList();

        Assert.NotNull(nextOccurrence);
        Assert.Equal(10, nextOccurrence.Value.Day);
        Assert.Equal(5, completed.Count);
    }

    [Fact]
    public void DailySchedule_WithIntervalStartInPast_ShouldCalculateCorrectNext()
    {
        var clock = CreateClock(2025, 8, 10, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 3)
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();

        Assert.NotNull(nextOccurrence);
        Assert.Equal(10, nextOccurrence.Value.Day);
    }

    [Fact]
    public void DailySchedule_DuringActiveOccurrence_ShouldReturnCurrentAsNext()
    {
        var clock = CreateClock(2025, 8, 1, 12, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();

        Assert.NotNull(nextOccurrence);
        Assert.Equal(1, nextOccurrence.Value.Day);
    }

    [Fact]
    public void DailySchedule_AfterDailyOccurrence_ShouldReturnTomorrow()
    {
        var clock = CreateClock(2025, 8, 1, 22, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var previousOccurrence = schedule.GetPreviousOccurrence();

        Assert.NotNull(nextOccurrence);
        Assert.NotNull(previousOccurrence);
        Assert.Equal(2, nextOccurrence.Value.Day);
        Assert.Equal(1, previousOccurrence.Value.Day);
    }

    [Fact]
    public void DailySchedule_AcrossMonthBoundary_ShouldWork()
    {
        var clock = CreateClock(2025, 7, 30, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 7, 30),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(5, upcoming.Count);
        Assert.Equal(7, upcoming[0].Month);
        Assert.Equal(7, upcoming[1].Month);
        Assert.Equal(8, upcoming[2].Month);
        Assert.Equal(8, upcoming[3].Month);
        Assert.Equal(8, upcoming[4].Month);
    }

    [Fact]
    public void DailySchedule_AcrossYearBoundary_ShouldWork()
    {
        var clock = CreateClock(2024, 12, 30, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 12, 30),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(5, upcoming.Count);
        Assert.Equal(2024, upcoming[0].Year);
        Assert.Equal(2024, upcoming[1].Year);
        Assert.Equal(2025, upcoming[2].Year);
        Assert.Equal(2025, upcoming[3].Year);
        Assert.Equal(2025, upcoming[4].Year);
    }

    [Fact]
    public void DailySchedule_LeapYear_ShouldHandleFebruary29()
    {
        var clock = CreateClock(2024, 2, 28, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 2, 28),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        Assert.Equal(28, upcoming[0].Day);
        Assert.Equal(29, upcoming[1].Day);
        Assert.Equal(1, upcoming[2].Day);
        Assert.Equal(3, upcoming[2].Month);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(30)]
    public void DailySchedule_VariousIntervals_ShouldWork(int interval)
    {
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily(o => o.Interval = interval)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.Equal(interval, diff.Days);
        }
    }

    [Fact]
    public void DailySchedule_Description_ShouldBeCorrect()
    {
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var dailySchedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();
            
        var intervalSchedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 3)
            .Build();

        var dailyDescription = dailySchedule.Description;
        var intervalDescription = intervalSchedule.Description;

        Assert.Contains("daily", dailyDescription);
        Assert.Contains("every 3 days", intervalDescription);
        Assert.Contains("9:00 AM - 5:00 PM", dailyDescription);
        Assert.Contains("August 1st, 2025", dailyDescription);
    }
}