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
        // Arrange - Start on 15th of July
        var clock = CreateClock(2025, 7, 15, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 7, 15),
            TestTime(20, 0),
            TestTime(21, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Monthly().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal("Monthly", schedule.Type);
        Assert.NotNull(nextOccurrence);
        Assert.Equal(4, upcoming.Count);
        
        // Should be on 15th of each month
        Assert.Equal(15, upcoming[0].Day); // July 15
        Assert.Equal(15, upcoming[1].Day); // August 15
        Assert.Equal(15, upcoming[2].Day); // September 15
        Assert.Equal(15, upcoming[3].Day); // October 15
        
        // Verify month progression
        Assert.Equal(7, upcoming[0].Month);
        Assert.Equal(8, upcoming[1].Month);
        Assert.Equal(9, upcoming[2].Month);
        Assert.Equal(10, upcoming[3].Month);
    }

    [Fact]
    public void MonthlySchedule_MultipleDaysOfMonth_ShouldReturnCorrectOccurrences()
    {
        // Arrange
        var clock = CreateClock(2025, 7, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 7, 1),
            TestTime(12, 0),
            TestTime(12, 30),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 1, 15, 31 })))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(9).ToList();

        // Assert
        // July: 1, 15, 31; August: 1, 15, 31; September: 1, 15 (no 31st)
        Assert.Equal(9, upcoming.Count);
        
        // July occurrences
        Assert.Equal(1, upcoming[0].Day);
        Assert.Equal(15, upcoming[1].Day);
        Assert.Equal(31, upcoming[2].Day);
        Assert.Equal(7, upcoming[0].Month);
        Assert.Equal(7, upcoming[1].Month);
        Assert.Equal(7, upcoming[2].Month);
        
        // August occurrences
        Assert.Equal(1, upcoming[3].Day);
        Assert.Equal(15, upcoming[4].Day);
        Assert.Equal(31, upcoming[5].Day);
        Assert.Equal(8, upcoming[3].Month);
        
        // September occurrences (no 31st)
        Assert.Equal(1, upcoming[6].Day);
        Assert.Equal(15, upcoming[7].Day);
        Assert.Equal(9, upcoming[6].Month);
        Assert.Equal(9, upcoming[7].Month);
    }

    [Fact]
    public void MonthlySchedule_Day31InFebruary_ShouldSkipInvalidDates()
    {
        // Arrange - Start in January with day 31
        var clock = CreateClock(2025, 1, 31, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 31),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Monthly().Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        // Should skip February (no 31st) and March should have 31st
        Assert.Equal(6, upcoming.Count);
        Assert.Equal(1, upcoming[0].Month); // Jan 31
        Assert.Equal(3, upcoming[1].Month); // Mar 31 (skip Feb)
        Assert.Equal(5, upcoming[2].Month); // May 31 (skip Apr)
        Assert.Equal(7, upcoming[3].Month); // Jul 31 (skip Jun)
        Assert.Equal(8, upcoming[4].Month); // Aug 31
        Assert.Equal(10, upcoming[5].Month); // Oct 31 (skip Sep)
    }

    [Fact]
    public void MonthlySchedule_RelativeFirstFriday_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(18, 0),
            TestTime(21, 30),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);
        
        // All should be Fridays
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Friday, occurrence.DayOfWeek);
        }
        
        // Should be first Friday of each month
        Assert.Equal(1, upcoming[0].Day);  // Aug 1, 2025 is first Friday
        Assert.Equal(5, upcoming[1].Day);  // Sep 5, 2025 is first Friday  
        Assert.Equal(3, upcoming[2].Day);  // Oct 3, 2025 is first Friday
        Assert.Equal(7, upcoming[3].Day);  // Nov 7, 2025 is first Friday
    }

    [Fact]
    public void MonthlySchedule_RelativeLastWeekendDay_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(14, 0),
            TestTime(16, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // All should be weekend days (Saturday or Sunday)
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.DayOfWeek == IsoDayOfWeek.Saturday || 
                       occurrence.DayOfWeek == IsoDayOfWeek.Sunday);
        }
        
        // Should be last weekend day of each month
        // Verify they are towards the end of each month
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.Day >= 25); // Should be in last week
        }
    }

    [Fact]
    public void MonthlySchedule_RelativeSecondTuesday_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Second, RelativePosition.Tuesday))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);
        
        // All should be Tuesdays
        foreach (var occurrence in upcoming)
        {
            Assert.Equal(IsoDayOfWeek.Tuesday, occurrence.DayOfWeek);
        }
        
        // Should be second Tuesday of each month (typically days 8-14)
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.Day >= 8 && occurrence.Day <= 14);
        }
    }

    [Fact]
    public void MonthlySchedule_RelativeThirdWeekday_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 8, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 8, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Third, RelativePosition.Weekday))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);
        
        // All should be weekdays (Monday-Friday)
        foreach (var occurrence in upcoming)
        {
            Assert.True(occurrence.DayOfWeek >= IsoDayOfWeek.Monday && 
                       occurrence.DayOfWeek <= IsoDayOfWeek.Friday);
        }
    }

    [Fact]
    public void MonthlySchedule_RelativeLastDay_ShouldWork()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(23, 0),
            TestTime(23, 59),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.Last, RelativePosition.Day))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(12).ToList();

        // Assert
        Assert.Equal(12, upcoming.Count);
        
        // Should be last day of each month
        var expectedLastDays = new[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 }; // 2025 is not a leap year
        
        for (int i = 0; i < 12; i++)
        {
            Assert.Equal(expectedLastDays[i], upcoming[i].Day);
            Assert.Equal(i + 1, upcoming[i].Month);
        }
    }

    [Fact]
    public void MonthlySchedule_WithInterval_ShouldSkipMonths()
    {
        // Arrange - Every 3 months (quarterly)
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(14, 0),
            TestTime(16, 0),
            TestTimeZone);

        var schedule = builder.Recurring()
            .Monthly(o =>
            {
                o.Interval = 3;
                o.UseRelative(RelativeIndex.Last, RelativePosition.WeekendDay);
            }).Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);
        
        // Should be every 3 months: Jan, Apr, Jul, Oct
        Assert.Equal(1, upcoming[0].Month);
        Assert.Equal(4, upcoming[1].Month);
        Assert.Equal(7, upcoming[2].Month);
        Assert.Equal(10, upcoming[3].Month);
    }

    [Fact]
    public void MonthlySchedule_AcrossYearBoundary_ShouldWork()
    {
        // Arrange - Start in December
        var clock = CreateClock(2024, 12, 15, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 12, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Monthly().Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);
        
        // Should cross year boundary
        Assert.Equal(2024, upcoming[0].Year); // Dec 2024
        Assert.Equal(2025, upcoming[1].Year); // Jan 2025
        Assert.Equal(2025, upcoming[2].Year); // Feb 2025
        Assert.Equal(2025, upcoming[3].Year); // Mar 2025
        
        Assert.Equal(12, upcoming[0].Month);
        Assert.Equal(1, upcoming[1].Month);
        Assert.Equal(2, upcoming[2].Month);
        Assert.Equal(3, upcoming[3].Month);
    }

    [Fact]
    public void MonthlySchedule_LeapYear_February29_ShouldWork()
    {
        // Arrange - Leap year 2024
        var clock = CreateClock(2024, 2, 29, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2024, 2, 29),
            TestTime(12, 0),
            TestTime(13, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Monthly().Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        // Should skip non-leap years and months without 29th day
        Assert.Equal(5, upcoming.Count);
        Assert.Equal(29, upcoming[0].Day); // Feb 29, 2024
        // Should skip to months that have 29th or next leap year
    }

    [Fact]
    public void MonthlySchedule_InvalidDaysOfMonth_ShouldIgnore()
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
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 0, 1, 32, 15, -1, 31 }))) // Mix of valid/invalid
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        // Should only use valid days (1, 15, 31)
        var validOccurrences = upcoming.Where(o => o.Day == 1 || o.Day == 15 || o.Day == 31).ToList();
        Assert.Equal(upcoming.Count, validOccurrences.Count);
    }

    [Fact]
    public void MonthlySchedule_StartInPast_ShouldCalculateCorrectNext()
    {
        // Arrange - Clock set to April, schedule started in January
        var clock = CreateClock(2025, 4, 10, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 1, 15),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone);

        var schedule = builder.Recurring().Monthly().Build();

        // Act
        var nextOccurrence = schedule.GetNextOccurrence();
        var completed = schedule.GetOccurrencesCompleted(10).ToList();

        // Assert
        Assert.NotNull(nextOccurrence);
        Assert.Equal(15, nextOccurrence.Value.Day);
        Assert.Equal(4, nextOccurrence.Value.Month); // April 15th should be next
        
        // Should have completed Jan 15, Feb 15, Mar 15
        Assert.Equal(3, completed.Count);
    }

    [Fact]
    public void MonthlySchedule_Description_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 7, 15, 8, 0);
        var context = CreateContext(clock);
        
        var builder = context.CreateBuilder(
            TestDate(2025, 7, 15),
            TestTime(20, 0),
            TestTime(21, 0),
            TestTimeZone);

        var monthlySchedule = builder.Recurring().Monthly().Build();
        var multiDaySchedule = builder.Recurring()
            .Monthly(o => o.UseDaysOfMonth(list => list.AddRange(new[] { 1, 15, 31 })))
            .Build();
        var relativeSchedule = builder.Recurring()
            .Monthly(o => o.UseRelative(RelativeIndex.First, RelativePosition.Friday))
            .Build();

        // Act
        var monthlyDescription = monthlySchedule.Description;
        var multiDayDescription = multiDaySchedule.Description;
        var relativeDescription = relativeSchedule.Description;

        // Assert
        Assert.Contains("monthly", monthlyDescription);
        Assert.Contains("on the 15th", monthlyDescription);
        Assert.Contains("on the 1st, 15th, and 31st", multiDayDescription);
        Assert.Contains("on the first friday", relativeDescription);
        Assert.Contains("8:00 PM - 9:00 PM", monthlyDescription);
    }
}