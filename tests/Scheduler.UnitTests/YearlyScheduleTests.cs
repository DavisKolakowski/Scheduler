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
        // Arrange - Start on Christmas
        var clock = CreateClock(2024, 12, 25, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Yearly().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal("Yearly", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Equal(4, upcoming.Count);
        
        // Should be Dec 25th of consecutive years
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
        // Arrange
        var clock = CreateClock(2025, 6, 15, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(10, 0),
            TestTime(16, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 }))) // June and December
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // Should alternate between June and December
        Assert.Equal(6, upcoming[0].Month);  // June 2025
        Assert.Equal(12, upcoming[1].Month); // December 2025
        Assert.Equal(6, upcoming[2].Month);  // June 2026
        Assert.Equal(12, upcoming[3].Month); // December 2026
        Assert.Equal(6, upcoming[4].Month);  // June 2027
        Assert.Equal(12, upcoming[5].Month); // December 2027
        
        // All should be on 15th
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(15, occurrence.Day);
        }
    }

    [Fact]
    public void YearlySchedule_MultipleDaysInMonth_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 2, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 2, 1),
            TestTime(9, 0),
            TestTime(10, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 10, 20 }))) // 10th and 20th of February
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // Should be Feb 10, Feb 20, Feb 10 (next year), Feb 20 (next year), etc.
        Assert.Equal(10, upcoming[0].Day);
        Assert.Equal(20, upcoming[1].Day);
        Assert.Equal(10, upcoming[2].Day);
        Assert.Equal(20, upcoming[3].Day);
        
        Assert.Equal(2, upcoming[0].Month); // All February
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
        // Arrange
        var clock = CreateClock(2024, 1, 6, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 1, 6),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 12 })); // January and December
                o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay);
            }).Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // All should be weekend days
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.DayOfWeek == IsoDayOfWeek.Saturday || 
                       occurrence.DayOfWeek == IsoDayOfWeek.Sunday);
        }
        
        // Should alternate between January and December
        Assert.Equal(1, upcoming[0].Month);  // January
        Assert.Equal(12, upcoming[1].Month); // December
        Assert.Equal(1, upcoming[2].Month);  // January (next year)
        Assert.Equal(12, upcoming[3].Month); // December (next year)
        
        // Should be first weekend day of month (typically days 1-7)
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.Day <= 7);
        }
    }

    [Fact]
    public void YearlySchedule_WithInterval_ShouldSkipYears()
    {
        // Arrange - Every 2 years
        var clock = CreateClock(2024, 1, 6, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 1, 6),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o =>
            {
                o.Interval = 2;
                o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 12 }));
                o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay);
            }).Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // Should be every 2 years: 2024, 2024, 2026, 2026, 2028, 2028
        Assert.Equal(2024, upcoming[0].Year);
        Assert.Equal(2024, upcoming[1].Year);
        Assert.Equal(2026, upcoming[2].Year);
        Assert.Equal(2026, upcoming[3].Year);
        Assert.Equal(2028, upcoming[4].Year);
        Assert.Equal(2028, upcoming[5].Year);
        
        // Should alternate between January and December within each year
        Assert.Equal(1, upcoming[0].Month);
        Assert.Equal(12, upcoming[1].Month);
        Assert.Equal(1, upcoming[2].Month);
        Assert.Equal(12, upcoming[3].Month);
    }

    [Fact]
    public void YearlySchedule_LeapYear_February29_ShouldWork()
    {
        // Arrange - Start on leap day
        var clock = CreateClock(2024, 2, 29, 8, 0); // 2024 is leap year
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 2, 29),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Yearly().Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        // Should only occur in leap years
        Assert.True(upcoming.Count <= 5); // May be fewer if not all years are leap years
        
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(2, occurrence.Month); // February
            Assert.Equal(29, occurrence.Day);  // 29th
            
            // Verify it's a leap year
            var year = occurrence.Year;
            var isLeapYear = (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
            Assert.True(isLeapYear);
        }
    }

    [Fact]
    public void YearlySchedule_RelativeLastSunday_InMultipleMonths_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 3, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 3, 1),
            TestTime(14, 0),
            TestTime(15, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o =>
            {
                o.UseMonthsOfYear(list => list.AddRange(new[] { 3, 10 })); // March and October
                o.UseRelative(RelativeIndex.Last, RelativePosition.Sunday);
            }).Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // All should be Sundays
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Sunday, occurrence.DayOfWeek);
        }
        
        // Should alternate between March and October
        Assert.Equal(3, upcoming[0].Month);
        Assert.Equal(10, upcoming[1].Month);
        Assert.Equal(3, upcoming[2].Month);
        Assert.Equal(10, upcoming[3].Month);
        
        // Should be last Sunday of each month (day >= 25)
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.Day >= 25);
        }
    }

    [Fact]
    public void YearlySchedule_AllMonths_Day15_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 15, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(24).ToList(); // 2 years worth

        // Assert
        Assert.Equal(24, upcoming.Count);
        
        // Should be 15th of every month
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(15, occurrence.Day);
        }
        
        // Should cover all 12 months twice
        var months = upcoming.Select(o => o.Month).ToList();
        for (int month = 1; month <= 12; month++)
        {
            Assert.Equal(2, months.Count(m => m == month)); // Should appear twice
        }
    }

    [Fact]
    public void YearlySchedule_InvalidMonths_ShouldIgnore()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 0, 1, 13, 6, -1, 12 }))) // Mix of valid/invalid
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        // Should only use valid months (1, 6, 12)
        var validMonths = new[] { 1, 6, 12 };
        foreach (var occurrence in upcoming)
        {
            Assert.Contains(occurrence.Month, validMonths);
        }
    }

    [Fact]
    public void YearlySchedule_StartInPast_ShouldCalculateCorrectNext()
    {
        // Arrange - Clock set to 2027, schedule started in 2024
        var clock = CreateClock(2027, 6, 10, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Yearly().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var completed = schedule.GetOccurrencesCompleted(10).ToList();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal(25, nextOccurrence.Value.Day);
        Assert.Equal(12, nextOccurrence.Value.Month);
        Assert.Equal(2027, nextOccurrence.Value.Year); // Next Christmas
        
        // Should have completed 2024, 2025, 2026
        Assert.Equal(3, completed.Count);
    }

    [Fact]
    public void YearlySchedule_InvalidDaysOfMonth_ShouldSkipInvalidDates()
    {
        // Arrange - Use day 31 but include February
        var clock = CreateClock(2025, 1, 31, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 31),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 1, 2, 3, 4 }))) // Jan, Feb, Mar, Apr
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(8).ToList();

        // Assert
        // Should skip February (no 31st) and April (no 31st)
        var validMonths = upcoming.Select(o => o.Month).Distinct().ToList();
        Assert.Contains(1, validMonths); // January has 31st
        Assert.Contains(3, validMonths); // March has 31st
        Assert.DoesNotContain(2, validMonths); // February doesn't have 31st
        Assert.DoesNotContain(4, validMonths); // April doesn't have 31st
    }

    [Fact]
    public void YearlySchedule_EveryFiveYears_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2020, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2020, 1, 1),
            TestTime(0, 0),
            TestTime(23, 59),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o => o.Interval = 5) // Every 5 years
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        Assert.Equal(5, upcoming.Count);
        
        // Should be 2020, 2025, 2030, 2035, 2040
        Assert.Equal(2020, upcoming[0].Year);
        Assert.Equal(2025, upcoming[1].Year);
        Assert.Equal(2030, upcoming[2].Year);
        Assert.Equal(2035, upcoming[3].Year);
        Assert.Equal(2040, upcoming[4].Year);
    }

    [Fact]
    public void YearlySchedule_Description_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2024, 12, 25, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone);

        var yearlySchedule = builder.Recurring().Yearly().Build();
        var multiMonthSchedule = builder.Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })))
            .Build();
        var relativeSchedule = builder.Recurring()
            .Yearly(o => o.UseRelative(RelativeIndex.First, RelativePosition.WeekendDay))
            .Build();

        // Act
        var yearlyDescription = yearlySchedule.Description;
        var multiMonthDescription = multiMonthSchedule.Description;
        var relativeDescription = relativeSchedule.Description;

        // Assert
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
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Yearly(o => o.UseRelative(index, RelativePosition.Monday))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        
        // All should be Mondays
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Monday, occurrence.DayOfWeek);
        }
    }
}