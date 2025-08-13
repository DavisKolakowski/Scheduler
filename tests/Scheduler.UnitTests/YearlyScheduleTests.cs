using NodaTime;
using Scheduler.Core.Enums;

namespace Scheduler.UnitTests;

public class YearlyScheduleTests : BaseScheduleTests
{
    [Fact]
    public void YearlySchedule_BasicYearly_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2024, 12, 25, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Yearly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal("Yearly", schedule.Type);
        Assert.Equal(4, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(25, o.Day));
        Assert.All(upcoming, o => Assert.Equal(12, o.Month));
        Assert.Equal(new[] { 2024, 2025, 2026, 2027 }, upcoming.Select(o => o.Year).ToArray());
    }

    [Fact]
    public void YearlySchedule_MultipleMonths_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2025, 6, 15, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 6, 15),
            TestTime(10, 0),
            TestTime(16, 0),
            TestTimeZone)
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        Assert.Equal(new[] { 6, 12, 6, 12, 6, 12 }, upcoming.Select(o => o.Month).ToArray());
        Assert.All(upcoming, o => Assert.Equal(15, o.Day));
    }

    [Fact]
    public void YearlySchedule_MultipleDaysInMonth_ShouldWork()
    {
        var clock = CreateClock(2025, 2, 1, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 2, 1),
            TestTime(9, 0),
            TestTime(10, 0),
            TestTimeZone)
            .Yearly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 10, 20 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        Assert.Equal(new[] { 10, 20, 10, 20 }, upcoming.Take(4).Select(o => o.Day));
        Assert.All(upcoming.Take(4), o => Assert.Equal(2, o.Month));
    }

    [Fact]
    public void YearlySchedule_RelativeFirstWeekendDay_ShouldWork()
    {
        var clock = CreateClock(2024, 1, 6, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2024, 1, 6),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 12 }));
                o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        foreach (var occ in upcoming)
        {
            Assert.True(occ.DayOfWeek == IsoDayOfWeek.Saturday || occ.DayOfWeek == IsoDayOfWeek.Sunday);
        }
        Assert.Equal(new[] { 1, 12, 1, 12 }, upcoming.Take(4).Select(o => o.Month));
        Assert.All(upcoming, o => Assert.True(o.Day <= 7));
    }

    [Fact]
    public void YearlySchedule_WithInterval_ShouldSkipYears()
    {
        var clock = CreateClock(2024, 1, 6, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2024, 1, 6),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Yearly(o =>
            {
                o.Interval = 2;
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 12 }));
                o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        Assert.Equal(new[] { 2024, 2024, 2026, 2026, 2028, 2028 }, upcoming.Select(o => o.Year).ToArray());
    }

    [Fact]
    public void YearlySchedule_LeapYear_February29_ShouldWork()
    {
        var clock = CreateClock(2024, 2, 29, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2024, 2, 29),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Yearly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.True(upcoming.Count <= 3);
        Assert.All(upcoming, o =>
        {
            Assert.Equal(2, o.Month);
            Assert.Equal(29, o.Day);
        });
    }

    [Fact]
    public void YearlySchedule_RelativeLastSunday_InMultipleMonths_ShouldWork()
    {
        var clock = CreateClock(2025, 3, 1, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 3, 1),
            TestTime(14, 0),
            TestTime(15, 0),
            TestTimeZone)
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.AddRange(new[] { 3, 10 }));
                o.UseRelative(RelativeIndex.Last, RelativePosition.Sunday);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Sunday, o.DayOfWeek));
        Assert.All(upcoming, o => Assert.True(o.Day >= 25));
    }

    [Fact]
    public void YearlySchedule_AllMonths_Day15_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 15, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 1, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(Enumerable.Range(1, 12))))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(24).ToList();

        Assert.Equal(24, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(15, o.Day));
        var months = upcoming.Select(o => o.Month).ToList();
        for (int m = 1; m <= 12; m++)
        {
            Assert.Equal(2, months.Count(x => x == m));
        }
    }

    [Fact]
    public void YearlySchedule_InvalidMonths_ShouldIgnore()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 0, 1, 13, 6, -1, 12 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();
        var validMonths = new[] { 1, 6, 12 };
        Assert.All(upcoming, o => Assert.Contains(o.Month, validMonths));
    }

    [Fact]
    public void YearlySchedule_StartInPast_ShouldCalculateCorrectNext()
    {
        var clock = CreateClock(2027, 6, 10, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2024, 12, 25),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Yearly()
            .Build();

        var next = schedule.NextOccurrence;
        var completed = schedule.GetCompletedOccurrences(10).ToList();

        Assert.NotNull(next);
        Assert.Equal(25, next.Value.Day);
        Assert.Equal(12, next.Value.Month);
        Assert.Equal(2027, next.Value.Year);
        Assert.Equal(3, completed.Count);
    }

    [Fact]
    public void YearlySchedule_InvalidDaysOfMonth_ShouldSkipInvalidDates()
    {
        var clock = CreateClock(2025, 1, 31, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 1, 31),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 2, 3, 4 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(8).ToList();
        var months = upcoming.Select(o => o.Month).Distinct().ToList();
        Assert.Contains(1, months);
        Assert.Contains(3, months);
        Assert.DoesNotContain(2, months);
        Assert.DoesNotContain(4, months);
    }

    [Fact]
    public void YearlySchedule_EveryFiveYears_ShouldWork()
    {
        var clock = CreateClock(2020, 1, 1, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2020, 1, 1),
            TestTime(0, 0),
            TestTime(23, 59),
            TestTimeZone)
            .Yearly(o => o.Interval = 5)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();
        Assert.Equal(new[] { 2020, 2025, 2030, 2035, 2040 }, upcoming.Select(o => o.Year).ToArray());
    }

    [Fact]
    public void YearlySchedule_Description_ShouldBeCorrect()
    {
        var clock = CreateClock(2024, 12, 25, 8, 0);
        var factory = CreateFactory(clock);
        
        var yearly = factory.Create(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Yearly()
            .Build();

        var multiMonth = factory.Create(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })))
            .Build();

        var relative = factory.Create(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Yearly(o => o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay))
            .Build();

        Assert.Contains("yearly", yearly.Description);
        Assert.Contains("December", yearly.Description);
        Assert.Contains("25th", yearly.Description);
        Assert.Contains("in June and December", multiMonth.Description);
        Assert.Contains("first weekend day", relative.Description.ToLower());
        Assert.Contains("8:00 AM - 8:00 PM", yearly.Description);
    }

    [Theory]
    [InlineData(RelativeIndex.First)]
    [InlineData(RelativeIndex.Second)]
    [InlineData(RelativeIndex.Third)]
    [InlineData(RelativeIndex.Fourth)]
    [InlineData(RelativeIndex.Last)]
    public void YearlySchedule_AllRelativeIndexes_ShouldWork(RelativeIndex index)
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var factory = CreateFactory(clock);
        
        var schedule = factory.Create(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Yearly(o => o.UseRelative(index, RelativePosition.Monday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();
        Assert.Equal(3, upcoming.Count);
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Monday, o.DayOfWeek));
    }
}