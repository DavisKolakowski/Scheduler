using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Enums;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class MonthlyScheduleTests : BaseScheduleTests
{
    [Fact]
    public void MonthlySchedule_BasicMonthly_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2025, 7, 15, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 7, 15),
            TestTime(20, 0),
            TestTime(21, 0),
            TestTimeZone)
            .Recurring()
            .Monthly()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal("Monthly", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Equal(4, upcoming.Count);
        
        Assert.Equal(15, upcoming[0].Day);
        Assert.Equal(15, upcoming[1].Day);
        Assert.Equal(15, upcoming[2].Day);
        Assert.Equal(15, upcoming[3].Day);
        
        Assert.Equal(7, upcoming[0].Month);
        Assert.Equal(8, upcoming[1].Month);
        Assert.Equal(9, upcoming[2].Month);
        Assert.Equal(10, upcoming[3].Month);
    }

    [Fact]
    public void MonthlySchedule_MultipleDaysOfMonth_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2025, 7, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 7, 1),
            TestTime(12, 0),
            TestTime(12, 30),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 1, 15, 31 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(9).ToList();

        Assert.Equal(9, upcoming.Count);
        
        Assert.Equal(1, upcoming[0].Day);
        Assert.Equal(15, upcoming[1].Day);
        Assert.Equal(31, upcoming[2].Day);
        Assert.Equal(7, upcoming[0].Month);
        Assert.Equal(7, upcoming[1].Month);
        Assert.Equal(7, upcoming[2].Month);
        
        Assert.Equal(1, upcoming[3].Day);
        Assert.Equal(15, upcoming[4].Day);
        Assert.Equal(31, upcoming[5].Day);
        Assert.Equal(8, upcoming[3].Month);
        
        Assert.Equal(1, upcoming[6].Day);
        Assert.Equal(15, upcoming[7].Day);
        Assert.Equal(9, upcoming[6].Month);
        Assert.Equal(9, upcoming[7].Month);
    }

    [Fact]
    public void MonthlySchedule_Day31InFebruary_ShouldSkipInvalidDates()
    {
        var clock = CreateClock(2025, 1, 31, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 31),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Recurring()
            .Monthly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        Assert.Equal(1, upcoming[0].Month);
        Assert.Equal(3, upcoming[1].Month);
        Assert.Equal(5, upcoming[2].Month);
        Assert.Equal(7, upcoming[3].Month);
        Assert.Equal(8, upcoming[4].Month);
        Assert.Equal(10, upcoming[5].Month);
    }

    [Fact]
    public void MonthlySchedule_RelativeFirstFriday_ShouldWork()
    {
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(18, 0),
            TestTime(21, 30),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Friday, occurrence.DayOfWeek);
        }
        
        Assert.Equal(1, upcoming[0].Day);
        Assert.Equal(5, upcoming[1].Day);
        Assert.Equal(3, upcoming[2].Day);
        Assert.Equal(7, upcoming[3].Day);
    }

    [Fact]
    public void MonthlySchedule_RelativeLastWeekendDay_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(14, 0),
            TestTime(16, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.DayOfWeek == IsoDayOfWeek.Saturday || 
                       occurrence.DayOfWeek == IsoDayOfWeek.Sunday);
        }
        
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.Day >= 25);
        }
    }

    [Fact]
    public void MonthlySchedule_RelativeSecondTuesday_ShouldWork()
    {
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Second, RelativePosition.Tuesday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Tuesday, occurrence.DayOfWeek);
        }
        
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.Day >= 8 && occurrence.Day <= 14);
        }
    }

    [Fact]
    public void MonthlySchedule_RelativeThirdWeekday_ShouldWork()
    {
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Third, RelativePosition.Weekday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.DayOfWeek >= IsoDayOfWeek.Monday && 
                       occurrence.DayOfWeek <= IsoDayOfWeek.Friday);
        }
    }

    [Fact]
    public void MonthlySchedule_RelativeLastDay_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(23, 0),
            TestTime(23, 59),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.Day))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(12).ToList();

        Assert.Equal(12, upcoming.Count);
        
        var expectedLastDays = new[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        
        for (int i = 0; i < 12; i++)
        {
            Assert.Equal(expectedLastDays[i], upcoming[i].Day);
            Assert.Equal(i + 1, upcoming[i].Month);
        }
    }

    [Fact]
    public void MonthlySchedule_WithInterval_ShouldSkipMonths()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(14, 0),
            TestTime(16, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o =>
            {
                o.Interval = 3;
                o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        
        Assert.Equal(1, upcoming[0].Month);
        Assert.Equal(4, upcoming[1].Month);
        Assert.Equal(7, upcoming[2].Month);
        Assert.Equal(10, upcoming[3].Month);
    }

    [Fact]
    public void MonthlySchedule_AcrossYearBoundary_ShouldWork()
    {
        var clock = CreateClock(2024, 12, 15, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 12, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        
        Assert.Equal(2024, upcoming[0].Year);
        Assert.Equal(2025, upcoming[1].Year);
        Assert.Equal(2025, upcoming[2].Year);
        Assert.Equal(2025, upcoming[3].Year);
        
        Assert.Equal(12, upcoming[0].Month);
        Assert.Equal(1, upcoming[1].Month);
        Assert.Equal(2, upcoming[2].Month);
        Assert.Equal(3, upcoming[3].Month);
    }

    [Fact]
    public void MonthlySchedule_LeapYear_February29_ShouldWork()
    {
        var clock = CreateClock(2024, 2, 29, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 2, 29),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Recurring()
            .Monthly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(5, upcoming.Count);
        Assert.Equal(29, upcoming[0].Day);
    }

    [Fact]
    public void MonthlySchedule_InvalidDaysOfMonth_ShouldIgnore()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 0, 1, 32, 15, -1, 31 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        var validOccurrences = upcoming.Where(o => o.Day == 1 || o.Day == 15 || o.Day == 31).ToList();
        Assert.Equal(upcoming.Count, validOccurrences.Count);
    }

    [Fact]
    public void MonthlySchedule_StartInPast_ShouldCalculateCorrectNext()
    {
        var clock = CreateClock(2025, 4, 10, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var completed = schedule.GetOccurrencesCompleted(10).ToList();

        Assert.NotNull(nextOccurrence);
        Assert.Equal(15, nextOccurrence.Value.Day);
        Assert.Equal(4, nextOccurrence.Value.Month);
        
        Assert.Equal(3, completed.Count);
    }

    [Fact]
    public void MonthlySchedule_Description_ShouldBeCorrect()
    {
        var clock = CreateClock(2025, 7, 15, 8, 0);
        var context = CreateContext(clock);
        
        var monthlySchedule = context.CreateBuilder(
            TestDate(2025, 7, 15),
            TestTime(20, 0),
            TestTime(21, 0),
            TestTimeZone)
            .Recurring()
            .Monthly()
            .Build();
            
        var multiDaySchedule = context.CreateBuilder(
            TestDate(2025, 7, 15),
            TestTime(20, 0),
            TestTime(21, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 1, 15, 31 })))
            .Build();
            
        var relativeSchedule = context.CreateBuilder(
            TestDate(2025, 7, 15),
            TestTime(20, 0),
            TestTime(21, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday))
            .Build();

        var monthlyDescription = monthlySchedule.Description;
        var multiDayDescription = multiDaySchedule.Description;
        var relativeDescription = relativeSchedule.Description;

        Assert.Contains("monthly", monthlyDescription);
        Assert.Contains("on the 15th", monthlyDescription);
        Assert.Contains("on the 1st, 15th, and 31st", multiDayDescription);
        Assert.Contains("on the first friday", relativeDescription);
        Assert.Contains("8:00 PM - 9:00 PM", monthlyDescription);
    }
}