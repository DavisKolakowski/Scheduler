using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Enums;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class YearlyScheduleTests : BaseScheduleTests
{
    [Fact]
    public void YearlySchedule_BasicYearly_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2024, 12, 25, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Recurring()
            .Yearly()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        Assert.Equal("Yearly", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Equal(4, upcoming.Count);
        
        Assert.Equal(25, upcoming[0].Day);
        Assert.Equal(25, upcoming[1].Day);
        Assert.Equal(25, upcoming[2].Day);
        Assert.Equal(25, upcoming[3].Day);
        
        Assert.Equal(12, upcoming[0].Month);
        Assert.Equal(12, upcoming[1].Month);
        Assert.Equal(12, upcoming[2].Month);
        Assert.Equal(12, upcoming[3].Month);
        
        Assert.Equal(2024, upcoming[0].Year);
        Assert.Equal(2025, upcoming[1].Year);
        Assert.Equal(2026, upcoming[2].Year);
        Assert.Equal(2027, upcoming[3].Year);
    }

    [Fact]
    public void YearlySchedule_MultipleMonths_ShouldReturnCorrectOccurrences()
    {
        var clock = CreateClock(2025, 6, 15, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(10, 0),
            TestTime(16, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        Assert.Equal(6, upcoming[0].Month);
        Assert.Equal(12, upcoming[1].Month);
        Assert.Equal(6, upcoming[2].Month);
        Assert.Equal(12, upcoming[3].Month);
        Assert.Equal(6, upcoming[4].Month);
        Assert.Equal(12, upcoming[5].Month);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(15, occurrence.Day);
        }
    }

    [Fact]
    public void YearlySchedule_MultipleDaysInMonth_ShouldWork()
    {
        var clock = CreateClock(2025, 2, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 2, 1),
            TestTime(9, 0),
            TestTime(10, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 10, 20 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        Assert.Equal(10, upcoming[0].Day);
        Assert.Equal(20, upcoming[1].Day);
        Assert.Equal(10, upcoming[2].Day);
        Assert.Equal(20, upcoming[3].Day);
        
        Assert.Equal(2, upcoming[0].Month);
        Assert.Equal(2, upcoming[1].Month);
        Assert.Equal(2, upcoming[2].Month);
        Assert.Equal(2, upcoming[3].Month);
        
        Assert.Equal(2025, upcoming[0].Year);
        Assert.Equal(2025, upcoming[1].Year);
        Assert.Equal(2026, upcoming[2].Year);
        Assert.Equal(2026, upcoming[3].Year);
    }

    [Fact]
    public void YearlySchedule_RelativeFirstWeekendDay_ShouldWork()
    {
        var clock = CreateClock(2024, 1, 6, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 1, 6),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 12 }));
                o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.DayOfWeek == IsoDayOfWeek.Saturday || 
                       occurrence.DayOfWeek == IsoDayOfWeek.Sunday);
        }
        
        Assert.Equal(1, upcoming[0].Month);
        Assert.Equal(12, upcoming[1].Month);
        Assert.Equal(1, upcoming[2].Month);
        Assert.Equal(12, upcoming[3].Month);
        
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.Day <= 7);
        }
    }

    [Fact]
    public void YearlySchedule_WithInterval_ShouldSkipYears()
    {
        var clock = CreateClock(2024, 1, 6, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 1, 6),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o =>
            {
                o.Interval = 2;
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 12 }));
                o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        Assert.Equal(2024, upcoming[0].Year);
        Assert.Equal(2024, upcoming[1].Year);
        Assert.Equal(2026, upcoming[2].Year);
        Assert.Equal(2026, upcoming[3].Year);
        Assert.Equal(2028, upcoming[4].Year);
        Assert.Equal(2028, upcoming[5].Year);
        
        Assert.Equal(1, upcoming[0].Month);
        Assert.Equal(12, upcoming[1].Month);
        Assert.Equal(1, upcoming[2].Month);
        Assert.Equal(12, upcoming[3].Month);
    }

    [Fact]
    public void YearlySchedule_LeapYear_February29_ShouldWork()
    {
        var clock = CreateClock(2024, 2, 29, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 2, 29),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Recurring()
            .Yearly()
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.True(upcoming.Count <= 5);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(2, occurrence.Month);
            Assert.Equal(29, occurrence.Day);
            
            var year = occurrence.Year;
            var isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
            Assert.True(isLeapYear);
        }
    }

    [Fact]
    public void YearlySchedule_RelativeLastSunday_InMultipleMonths_ShouldWork()
    {
        var clock = CreateClock(2025, 3, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 1),
            TestTime(14, 0),
            TestTime(15, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.AddRange(new[] { 3, 10 }));
                o.UseRelative(RelativeIndex.Last, RelativePosition.Sunday);
            })
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        Assert.Equal(6, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Sunday, occurrence.DayOfWeek);
        }
        
        Assert.Equal(3, upcoming[0].Month);
        Assert.Equal(10, upcoming[1].Month);
        Assert.Equal(3, upcoming[2].Month);
        Assert.Equal(10, upcoming[3].Month);
        
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.Day >= 25);
        }
    }

    [Fact]
    public void YearlySchedule_AllMonths_Day15_ShouldWork()
    {
        var clock = CreateClock(2025, 1, 15, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(24).ToList();

        Assert.Equal(24, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(15, occurrence.Day);
        }
        
        var months = upcoming.Select(o => o.Month).ToList();
        for (int month = 1; month <= 12; month++)
        {
            Assert.Equal(2, months.Count(m => m == month));
        }
    }

    [Fact]
    public void YearlySchedule_InvalidMonths_ShouldIgnore()
    {
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 0, 1, 13, 6, -1, 12 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        var validMonths = new[] { 1, 6, 12 };
        foreach (var occurrence in upcoming)
        {
            Assert.Contains(occurrence.Month, validMonths);
        }
    }

    [Fact]
    public void YearlySchedule_StartInPast_ShouldCalculateCorrectNext()
    {
        var clock = CreateClock(2027, 6, 10, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Yearly()
            .Build();

        var nextOccurrence = schedule.GetNextOccurrence();
        var completed = schedule.GetOccurrencesCompleted(10).ToList();

        Assert.NotNull(nextOccurrence);
        Assert.Equal(25, nextOccurrence.Value.Day);
        Assert.Equal(12, nextOccurrence.Value.Month);
        Assert.Equal(2027, nextOccurrence.Value.Year);
        
        Assert.Equal(3, completed.Count);
    }

    [Fact]
    public void YearlySchedule_InvalidDaysOfMonth_ShouldSkipInvalidDates()
    {
        var clock = CreateClock(2025, 1, 31, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 31),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 2, 3, 4 })))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(8).ToList();

        var validMonths = upcoming.Select(o => o.Month).Distinct().ToList();
        Assert.Contains(1, validMonths);
        Assert.Contains(3, validMonths);
        Assert.DoesNotContain(2, validMonths);
        Assert.DoesNotContain(4, validMonths);
    }

    [Fact]
    public void YearlySchedule_EveryFiveYears_ShouldWork()
    {
        var clock = CreateClock(2020, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2020, 1, 1),
            TestTime(0, 0),
            TestTime(23, 59),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.Interval = 5)
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        Assert.Equal(5, upcoming.Count);
        
        Assert.Equal(2020, upcoming[0].Year);
        Assert.Equal(2025, upcoming[1].Year);
        Assert.Equal(2030, upcoming[2].Year);
        Assert.Equal(2035, upcoming[3].Year);
        Assert.Equal(2040, upcoming[4].Year);
    }

    [Fact]
    public void YearlySchedule_Description_ShouldBeCorrect()
    {
        var clock = CreateClock(2024, 12, 25, 8, 0);
        var context = CreateContext(clock);
        
        var yearlySchedule = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Recurring()
            .Yearly()
            .Build();
            
        var multiMonthSchedule = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })))
            .Build();
            
        var relativeSchedule = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay))
            .Build();

        var yearlyDescription = yearlySchedule.Description;
        var multiMonthDescription = multiMonthSchedule.Description;
        var relativeDescription = relativeSchedule.Description;

        Assert.Contains("yearly", yearlyDescription);
        Assert.Contains("December", yearlyDescription);
        Assert.Contains("25th", yearlyDescription);
        Assert.Contains("in June and December", multiMonthDescription);
        Assert.Contains("first weekend day", relativeDescription);
        Assert.Contains("8:00 AM - 8:00 PM", yearlyDescription);
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
        var context = CreateContext(clock);
        
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseRelative(index, RelativePosition.Monday))
            .Build();

        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        Assert.Equal(3, upcoming.Count);
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Monday, occurrence.DayOfWeek);
        }
    }
}