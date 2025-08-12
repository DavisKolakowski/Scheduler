using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Enums;

namespace Scheduler.UnitTests;

public class EnumAndRelativeTests : BaseScheduleTests
{
    [Theory]
    [InlineData(RelativeIndex.First)]
    [InlineData(RelativeIndex.Second)]
    [InlineData(RelativeIndex.Third)]
    [InlineData(RelativeIndex.Fourth)]
    [InlineData(RelativeIndex.Last)]
    public void Schedule_AllRelativeIndexes_ShouldWork(RelativeIndex index)
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(index, RelativePosition.Monday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Monday, o.DayOfWeek));
    }

    [Theory]
    [InlineData(RelativePosition.Monday)]
    [InlineData(RelativePosition.Tuesday)]
    [InlineData(RelativePosition.Wednesday)]
    [InlineData(RelativePosition.Thursday)]
    [InlineData(RelativePosition.Friday)]
    [InlineData(RelativePosition.Saturday)]
    [InlineData(RelativePosition.Sunday)]
    public void Schedule_AllDayOfWeekPositions_ShouldWork(RelativePosition position)
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, position))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        var expectedDayOfWeek = (IsoDayOfWeek)(int)position;
        Assert.All(upcoming, o => Assert.Equal(expectedDayOfWeek, o.DayOfWeek));
    }

    [Fact]
    public void Schedule_RelativePosition_Day_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
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
        }
    }

    [Fact]
    public void Schedule_RelativePosition_Weekday_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Weekday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        Assert.All(upcoming, o => 
            Assert.True(o.DayOfWeek >= IsoDayOfWeek.Monday && o.DayOfWeek <= IsoDayOfWeek.Friday));
    }

    [Fact]
    public void Schedule_RelativePosition_WeekendDay_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        Assert.All(upcoming, o => 
            Assert.True(o.DayOfWeek == IsoDayOfWeek.Saturday || o.DayOfWeek == IsoDayOfWeek.Sunday));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Schedule_WeeklySchedule_AllDaysOfWeek_ShouldWork(int dayOfWeek)
    {
        var clock = CreateClock(2025, 1, 6, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.Add(dayOfWeek)))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        var expectedDayOfWeek = (IsoDayOfWeek)dayOfWeek;
        Assert.All(upcoming, o => Assert.Equal(expectedDayOfWeek, o.DayOfWeek));
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(1, 3, 5)]
    [InlineData(6, 7)]
    [InlineData(2, 4)]
    public void Schedule_WeeklySchedule_MultipleDays_ShouldWork(params int[] daysOfWeek)
    {
        var clock = CreateClock(2025, 1, 6, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(daysOfWeek)))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        Assert.True(upcoming.Count > 0);
        
        var expectedDays = daysOfWeek.Select(d => (IsoDayOfWeek)d).ToHashSet();
        Assert.All(upcoming, o => Assert.Contains(o.DayOfWeek, expectedDays));
    }

    [Theory]
    [InlineData(1, 15, 31)]
    [InlineData(10, 20)]
    [InlineData(5)]
    [InlineData(1, 2, 3, 4, 5)]
    public void Schedule_MonthlySchedule_MultipleDays_ShouldWork(params int[] daysOfMonth)
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(daysOfMonth)))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        Assert.True(upcoming.Count > 0);
        
        var expectedDays = daysOfMonth.ToHashSet();
        Assert.All(upcoming, o => Assert.Contains(o.Day, expectedDays));
    }

    [Theory]
    [InlineData(1, 6, 12)]
    [InlineData(3, 6, 9, 12)]
    [InlineData(6)]
    [InlineData(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12)]
    public void Schedule_YearlySchedule_MultipleMonths_ShouldWork(params int[] months)
    {
        var clock = CreateClock(2025, 1, 15, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(months)))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(15).ToList();

        Assert.True(upcoming.Count > 0);
        
        var expectedMonths = months.ToHashSet();
        Assert.All(upcoming, o => Assert.Contains(o.Month, expectedMonths));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(30)]
    public void Schedule_DailySchedule_VariousIntervals_ShouldWork(int interval)
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Daily(o => o.Interval = interval)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(5, upcoming.Count);
        
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.Equal(interval, diff.Days);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(52)]
    public void Schedule_WeeklySchedule_VariousIntervals_ShouldWork(int interval)
    {
        var clock = CreateClock(2025, 1, 6, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Weekly(o => o.Interval = interval)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Monday, o.DayOfWeek));
        
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date, PeriodUnits.Days);
            Assert.Equal(interval * 7, diff.Days);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(12)]
    public void Schedule_MonthlySchedule_VariousIntervals_ShouldWork(int interval)
    {
        var clock = CreateClock(2025, 1, 15, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.Interval = interval)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(5, upcoming.Count);
        
        Assert.All(upcoming, o => Assert.Equal(15, o.Day));
        
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var monthDiff = Period.Between(
                upcoming[i].Date.With(DateAdjusters.StartOfMonth), 
                upcoming[i + 1].Date.With(DateAdjusters.StartOfMonth), PeriodUnits.Months).Months;
            Assert.Equal(interval, monthDiff);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(10)]
    public void Schedule_YearlySchedule_VariousIntervals_ShouldWork(int interval)
    {
        var clock = CreateClock(2025, 6, 15, 8, 0);
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.Interval = interval)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        
        Assert.All(upcoming, o => 
        {
            Assert.Equal(6, o.Month);
            Assert.Equal(15, o.Day);
        });
        
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var yearDiff = upcoming[i + 1].Year - upcoming[i].Year;
            Assert.Equal(interval, yearDiff);
        }
    }
}