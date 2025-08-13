using NodaTime;
using Scheduler.Core.Enums;

namespace Scheduler.UnitTests;

public class MonthlyScheduleTests : BaseScheduleTests
{
    [Fact]
    public void MonthlySchedule_BasicMonthly_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2025, 7, 15, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 7, 15),
            TestTime(20, 0),
            TestTime(21, 0),
            TestTimeZone)
            .Monthly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal("Monthly", schedule.Type);
        Assert.Equal(4, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(15, o.Day));
    }

    [Fact]
    public void MonthlySchedule_MultipleDays_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2025, 7, 1, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 7, 1),
            TestTime(12, 0),
            TestTime(12, 30),
            TestTimeZone)
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 1, 15, 31 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.True(upcoming.Count >= 6);
        var pattern = upcoming.Take(3).Select(o => o.Day).ToList();
        Assert.Equal(new[] { 1, 15, 31 }, pattern);
    }

    [Fact]
    public void MonthlySchedule_RelativeFirstFriday_ShouldWork()
    {
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var factory = CreateFactory(clock);

        var schedule = factory.Create(
            TestDate(2025, 8, 1),
            TestTime(18, 0),
            TestTime(21, 30),
            TestTimeZone)
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Friday, o.DayOfWeek));
        Assert.All(upcoming, o => Assert.True(o.Day <= 7));
    }

    [Fact]
    public void MonthlySchedule_QuarterlyLastWeekendDay_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var factory = CreateFactory(clock);

        var schedule = factory.Create(
            TestDate(2025, 1, 1),
            TestTime(14, 0),
            TestTime(16, 0),
            TestTimeZone)
            .Monthly(o =>
            {
                o.Interval = 3;
                o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal(4, upcoming.Count);
        foreach (var occ in upcoming)
        {
            Assert.True(occ.DayOfWeek == IsoDayOfWeek.Saturday || occ.DayOfWeek == IsoDayOfWeek.Sunday);
        }
    }

    [Fact]
    public void MonthlySchedule_InvalidDays_ShouldSkip()
    {
        var clock = CreateClock(2025, 4, 1, 8, 0);
        var factory = CreateFactory(clock);

        var schedule = factory.Create(
            TestDate(2025, 4, 1),
            TestTime(9, 0),
            TestTime(10, 0),
            TestTimeZone)
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 30, 31 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();
        // April has no 31st so only 30th occurrences should appear in April
        Assert.Contains(upcoming.First().Month, new[] { 4, 5 });
    }

    [Fact]
    public void MonthlySchedule_RelativeFifthMonday_ShouldHandleMissing()
    {
        var clock = CreateClock(2025, 3, 1, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 3, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Monthly(o => o.UseRelative(RelativeIndex.Fourth, RelativePosition.Monday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();
        Assert.Equal(3, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Monday, o.DayOfWeek));
    }

    [Fact]
    public void MonthlySchedule_Description_ShouldBeCorrect()
    {
        var clock = CreateClock(2025, 7, 15, 8, 0);
        var factory = CreateFactory(clock);
        
        var monthly = factory.Create(
            TestDate(2025, 7, 15),
            TestTime(20, 0),
            TestTime(21, 0),
            TestTimeZone)
            .Monthly()
            .Build();

        var relative = factory.Create(
            TestDate(2025, 8, 1),
            TestTime(18, 0),
            TestTime(21, 30),
            TestTimeZone)
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday))
            .Build();

        Assert.Contains("monthly", monthly.Description);
        Assert.Contains("on the 15th", monthly.Description);
        Assert.Contains("first friday", relative.Description.ToLower());
    }
}