using NodaTime;
using NodaTime.Testing;
using Scheduler.Core;
using Scheduler.Core.Contracts;
using Scheduler.Core.Options;

namespace Scheduler.UnitTests;

public class SchedulePropertiesTests : BaseScheduleTests
{
    [Fact]
    public void Schedule_Type_ShouldReturnCorrectType()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        // Act
        var oneTimeSchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(10, 0), TestTime(11, 0), TestTimeZone).Build();
        var dailySchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(10, 0), TestTime(11, 0), TestTimeZone).Recurring().Daily().Build();
        var weeklySchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(10, 0), TestTime(11, 0), TestTimeZone).Recurring().Weekly().Build();
        var monthlySchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(10, 0), TestTime(11, 0), TestTimeZone).Recurring().Monthly().Build();
        var yearlySchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(10, 0), TestTime(11, 0), TestTimeZone).Recurring().Yearly().Build();

        // Assert
        Assert.Equal("OneTime", oneTimeSchedule.Type);
        Assert.Equal("Daily", dailySchedule.Type);
        Assert.Equal("Weekly", weeklySchedule.Type);
        Assert.Equal("Monthly", monthlySchedule.Type);
        Assert.Equal("Yearly", yearlySchedule.Type);
    }

    [Theory]
    [InlineData(10, 0, 11, 0, "01:00")] // 1 hour
    [InlineData(10, 0, 10, 30, "00:30")] // 30 minutes
    [InlineData(9, 0, 17, 0, "08:00")] // 8 hours
    [InlineData(23, 0, 1, 0, "02:00")] // Overnight: 2 hours
    [InlineData(22, 30, 6, 15, "07:45")] // Overnight: 7 hours 45 minutes
    [InlineData(12, 0, 12, 0, "00:00")] // Zero duration
    public void Schedule_OccurrenceDuration_ShouldCalculateCorrectly(int startHour, int startMinute, int endHour, int endMinute, string expectedDuration)
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(startHour, startMinute),
            TestTime(endHour, endMinute),
            TestTimeZone).Build();

        // Assert
        Assert.Equal(expectedDuration, schedule.OccurrenceDuration);
    }

    [Fact]
    public void Schedule_Options_ShouldReturnCorrectOptions()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);
        var startDate = TestDate(2025, 1, 15);
        var startTime = TestTime(14, 30);
        var endTime = TestTime(16, 45);

        // Act
        var schedule = context.CreateBuilder(startDate, startTime, endTime, TestTimeZone).Build();

        // Assert
        Assert.Equal(startDate, schedule.Options.StartDate);
        Assert.Equal(startTime, schedule.Options.StartTime);
        Assert.Equal(endTime, schedule.Options.EndTime);
        Assert.Equal(TestTimeZone, schedule.Options.TimeZone);
    }

    [Fact]
    public void Schedule_Description_OneTime_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 3, 15),
            TestTime(14, 30),
            TestTime(16, 0),
            TestTimeZone).Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("Occurs once", description);
        Assert.Contains("March 15th, 2025", description);
        Assert.Contains("2:30 PM - 4:00 PM", description);
    }

    [Fact]
    public void Schedule_Description_Daily_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily()
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("daily", description);
        Assert.Contains("9:00 AM - 5:00 PM", description);
        Assert.Contains("January 1st, 2025", description);
    }

    [Fact]
    public void Schedule_Description_DailyWithInterval_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring()
            .Daily(o => o.Interval = 3)
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("every 3 days", description);
    }

    [Fact]
    public void Schedule_Description_DailyWithEndDate_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 1),
            TestTime(9, 0),
            TestTime(17, 0),
            TestTimeZone)
            .Recurring(TestDate(2025, 12, 31))
            .Daily()
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("daily", description);
        Assert.Contains("until December 31st, 2025", description);
    }

    [Fact]
    public void Schedule_Description_Weekly_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 6, 8, 0); // Monday
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6), // Monday
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Weekly()
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("weekly", description);
        Assert.Contains("on Monday", description);
    }

    [Fact]
    public void Schedule_Description_WeeklyMultipleDays_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 6, 8, 0); // Monday
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 6),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone)
            .Recurring()
            .Weekly(o => o.UseDaysOfWeek(list => list.AddRange(new[] { 1, 3, 5 }))) // Mon, Wed, Fri
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("weekly", description);
        Assert.Contains("Monday, Wednesday, and Friday", description);
    }

    [Fact]
    public void Schedule_Description_Monthly_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 15, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 15),
            TestTime(14, 0),
            TestTime(15, 0),
            TestTimeZone)
            .Recurring()
            .Monthly()
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("monthly", description);
        Assert.Contains("on the 15th", description);
    }

    [Fact]
    public void Schedule_Description_MonthlyRelative_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 3, 8, 0); // First Friday of January 2025
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, 3),
            TestTime(14, 0),
            TestTime(15, 0),
            TestTimeZone)
            .Recurring()
            .Monthly(o => o.UseRelative(Scheduler.Core.Enums.RelativeIndex.First, Scheduler.Core.Enums.RelativePosition.Friday))
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("monthly", description);
        Assert.Contains("first friday", description);
    }

    [Fact]
    public void Schedule_Description_Yearly_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2024, 12, 25, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2024, 12, 25),
            TestTime(8, 0),
            TestTime(20, 0),
            TestTimeZone)
            .Recurring()
            .Yearly()
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("yearly", description);
        Assert.Contains("December", description);
        Assert.Contains("25th", description);
    }

    [Fact]
    public void Schedule_Description_YearlyMultipleMonths_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 6, 15, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 6, 15),
            TestTime(10, 0),
            TestTime(16, 0),
            TestTimeZone)
            .Recurring()
            .Yearly(o => o.UseMonthsOfYear(list => list.AddRange(new[] { 6, 12 })))
            .Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains("yearly", description);
        Assert.Contains("in June and December", description);
        Assert.Contains("15th", description);
    }

    [Theory]
    [InlineData(1, "st")]
    [InlineData(2, "nd")]
    [InlineData(3, "rd")]
    [InlineData(4, "th")]
    [InlineData(11, "th")]
    [InlineData(12, "th")]
    [InlineData(13, "th")]
    [InlineData(21, "st")]
    [InlineData(22, "nd")]
    [InlineData(23, "rd")]
    [InlineData(31, "st")]
    public void Schedule_Description_OrdinalNumbers_ShouldBeCorrect(int day, string expectedSuffix)
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        // Act
        var schedule = context.CreateBuilder(
            TestDate(2025, 1, day),
            TestTime(10, 0),
            TestTime(11, 0),
            TestTimeZone).Build();

        // Assert
        var description = schedule.Description;
        Assert.Contains($"{day}{expectedSuffix}", description);
    }

    [Fact]
    public void Schedule_Description_TimeFormatting_ShouldBeCorrect()
    {
        // Arrange
        var clock = CreateClock(2025, 1, 1, 8, 0);
        var context = CreateContext(clock);

        // Act
        var morningSchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(9, 0), TestTime(10, 30), TestTimeZone).Build();
        var afternoonSchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(14, 15), TestTime(16, 45), TestTimeZone).Build();
        var eveningSchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(19, 0), TestTime(21, 0), TestTimeZone).Build();
        var midnightSchedule = context.CreateBuilder(TestDate(2025, 1, 1), TestTime(0, 0), TestTime(1, 0), TestTimeZone).Build();

        // Assert
        Assert.Contains("9:00 AM - 10:30 AM", morningSchedule.Description);
        Assert.Contains("2:15 PM - 4:45 PM", afternoonSchedule.Description);
        Assert.Contains("7:00 PM - 9:00 PM", eveningSchedule.Description);
        Assert.Contains("12:00 AM - 1:00 AM", midnightSchedule.Description);
    }
}