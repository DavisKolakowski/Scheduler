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
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
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
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        var expectedDayOfWeek = (IsoDayOfWeek)(int)position;
        Assert.All(upcoming, o => Assert.Equal(expectedDayOfWeek, o.DayOfWeek));
    }

    [Fact]
    public void Schedule_RelativePosition_Day_ShouldWork()
    {
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(12).ToList(); // One year

        // Assert
        Assert.Equal(12, upcoming.Count);
        
        // Should be last day of each month
        var expectedLastDays = new[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 }; // 2025 is not leap year
        for (int i = 0; i < 12; i++)
        {
            Assert.Equal(expectedLastDays[i], upcoming[i].Day);
        }
    }

    [Fact]
    public void Schedule_RelativePosition_Weekday_ShouldWork()
    {
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // All should be weekdays (Monday-Friday)
        Assert.All(upcoming, o => 
            Assert.True(o.DayOfWeek >= IsoDayOfWeek.Monday && o.DayOfWeek <= IsoDayOfWeek.Friday));
    }

    [Fact]
    public void Schedule_RelativePosition_WeekendDay_ShouldWork()
    {
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(6).ToList();

        // Assert
        Assert.Equal(6, upcoming.Count);
        
        // All should be weekend days (Saturday or Sunday)
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
        // Arrange
        var clock = CreateClock(2025, 1, 6, 8, 0); // Monday
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.Add(dayOfWeek)))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(4).ToList();

        // Assert
        Assert.Equal(4, upcoming.Count);
        var expectedDayOfWeek = (IsoDayOfWeek)dayOfWeek;
        Assert.All(upcoming, o => Assert.Equal(expectedDayOfWeek, o.DayOfWeek));
    }

    [Theory]
    [InlineData(1, 2, 3)] // Mon, Tue, Wed
    [InlineData(1, 3, 5)] // Mon, Wed, Fri
    [InlineData(6, 7)]    // Sat, Sun
    [InlineData(2, 4)]    // Tue, Thu
    public void Schedule_WeeklySchedule_MultipleDays_ShouldWork(params int[] daysOfWeek)
    {
        // Arrange
        var clock = CreateClock(2025, 1, 6, 8, 0); // Monday
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(daysOfWeek)))
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        // Assert
        Assert.True(upcoming.Count > 0);
        
        // All occurrences should be on the specified days
        var expectedDays = daysOfWeek.Select(d => (IsoDayOfWeek)d).ToHashSet();
        Assert.All(upcoming, o => Assert.Contains(o.DayOfWeek, expectedDays));
    }

    [Theory]
    [InlineData(1, 15, 31)] // 1st, 15th, 31st
    [InlineData(10, 20)]    // 10th, 20th
    [InlineData(5)]         // 5th only
    [InlineData(1, 2, 3, 4, 5)] // First 5 days
    public void Schedule_MonthlySchedule_MultipleDays_ShouldWork(params int[] daysOfMonth)
    {
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(10).ToList();

        // Assert
        Assert.True(upcoming.Count > 0);
        
        // All occurrences should be on the specified days (when they exist in the month)
        var expectedDays = daysOfMonth.ToHashSet();
        Assert.All(upcoming, o => Assert.Contains(o.Day, expectedDays));
    }

    [Theory]
    [InlineData(1, 6, 12)] // Jan, Jun, Dec
    [InlineData(3, 6, 9, 12)] // Quarterly
    [InlineData(6)] // June only
    [InlineData(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12)] // All months
    public void Schedule_YearlySchedule_MultipleMonths_ShouldWork(params int[] months)
    {
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(15).ToList();

        // Assert
        Assert.True(upcoming.Count > 0);
        
        // All occurrences should be in the specified months
        var expectedMonths = months.ToHashSet();
        Assert.All(upcoming, o => Assert.Contains(o.Month, expectedMonths));
    }

    [Theory]
    [InlineData(1)] // Every day
    [InlineData(2)] // Every other day
    [InlineData(7)] // Weekly
    [InlineData(14)] // Bi-weekly
    [InlineData(30)] // Monthly-ish
    public void Schedule_DailySchedule_VariousIntervals_ShouldWork(int interval)
    {
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        Assert.Equal(5, upcoming.Count);
        
        // Verify intervals
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date);
            Assert.Equal(interval, diff.Days);
        }
    }

    [Theory]
    [InlineData(1)] // Every week
    [InlineData(2)] // Bi-weekly
    [InlineData(4)] // Monthly-ish
    [InlineData(52)] // Yearly
    public void Schedule_WeeklySchedule_VariousIntervals_ShouldWork(int interval)
    {
        // Arrange
        var clock = CreateClock(2025, 1, 6, 8, 0); // Monday
        var context = CreateContext(clock);

        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Weekly(o => o.Interval = interval)
            .Build();

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        
        // All should be Mondays
        Assert.All(upcoming, o => Assert.Equal(IsoDayOfWeek.Monday, o.DayOfWeek));
        
        // Verify intervals (approximately)
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var diff = Period.Between(upcoming[i].Date, upcoming[i + 1].Date);
            Assert.Equal(interval * 7, diff.Days);
        }
    }

    [Theory]
    [InlineData(1)] // Every month
    [InlineData(2)] // Bi-monthly
    [InlineData(3)] // Quarterly
    [InlineData(6)] // Semi-annually
    [InlineData(12)] // Yearly
    public void Schedule_MonthlySchedule_VariousIntervals_ShouldWork(int interval)
    {
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(5).ToList();

        // Assert
        Assert.Equal(5, upcoming.Count);
        
        // All should be on 15th
        Assert.All(upcoming, o => Assert.Equal(15, o.Day));
        
        // Verify month intervals
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var monthDiff = Period.Between(
                upcoming[i].Date.With(DateAdjusters.StartOfMonth), 
                upcoming[i + 1].Date.With(DateAdjusters.StartOfMonth)).Months;
            Assert.Equal(interval, monthDiff);
        }
    }

    [Theory]
    [InlineData(1)] // Every year
    [InlineData(2)] // Bi-yearly
    [InlineData(4)] // Every 4 years
    [InlineData(10)] // Every decade
    public void Schedule_YearlySchedule_VariousIntervals_ShouldWork(int interval)
    {
        // Arrange
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

        // Act
        var upcoming = schedule.GetUpcomingOccurrences(3).ToList();

        // Assert
        Assert.Equal(3, upcoming.Count);
        
        // All should be June 15th
        Assert.All(upcoming, o => 
        {
            Assert.Equal(6, o.Month);
            Assert.Equal(15, o.Day);
        });
        
        // Verify year intervals
        for (int i = 0; i < upcoming.Count - 1; i++)
        {
            var yearDiff = upcoming[i + 1].Year - upcoming[i].Year;
            Assert.Equal(interval, yearDiff);
        }
    }
}